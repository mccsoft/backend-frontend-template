using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using MccSoft.WebHooks.Configuration;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Interceptors;
using MccSoft.WebHooks.Manager;
using MccSoft.WebHooks.Processing;
using MccSoft.WebHooks.Publisher;
using MccSoft.WebHooks.Signing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace MccSoft.WebHooks;

public static class WebHookRegistration
{
    public static Type? DbContextType { get; private set; }

    public static DbSet<WebHook<TSub>> WebHooks<TSub>(this DbContext dbContext)
        where TSub : WebHookSubscription => dbContext.Set<WebHook<TSub>>();

    public static DbSet<TSub> WebHookSubscriptions<TSub>(this DbContext dbContext)
        where TSub : WebHookSubscription => dbContext.Set<TSub>();

    /// <summary>
    /// Registers WebHook and WebHookSubscription entities for the specified subscription type <typeparamref name="TSub"/>.
    /// Must be called from the <c>OnModelCreating</c> method of your DbContext.
    /// </summary>
    /// <typeparam name="TSub">The type of WebHook subscription entity.</typeparam>
    /// <param name="builder">The EF Core model builder.</param>
    /// <param name="dbContextType">The concrete type of the DbContext.</param>
    /// <returns>The same model builder instance to support chaining.</returns>
    public static ModelBuilder AddWebHookEntities<TSub>(
        this ModelBuilder builder,
        Type dbContextType
    )
        where TSub : WebHookSubscription
    {
        DbContextType = dbContextType;

        builder.Entity<WebHook<TSub>>(e =>
        {
            e.ToTable("WebHooks");
        });

        builder.Entity<TSub>(e =>
        {
            e.ToTable("WebHookSubscriptions");
            e.Property(x => x.Headers)
                .HasConversion(
                    v =>
                        JsonSerializer.Serialize<Dictionary<string, string>>(
                            v,
                            (JsonSerializerOptions?)null
                        ),
                    v =>
                        JsonSerializer.Deserialize<Dictionary<string, string>>(
                            v,
                            (JsonSerializerOptions?)null
                        ) ?? new()
                );
        });

        return builder;
    }

    /// <summary>
    /// Registers WebHook-related services and configuration in the dependency injection container.
    /// Includes configuration of resilience policies, Hangfire filters, and WebHook processing components.
    /// </summary>
    /// <typeparam name="TSub">The type of WebHook subscription.</typeparam>
    /// <param name="serviceCollection">The service collection to which the services will be added.</param>
    /// <param name="builder">Optional configuration delegate for setting WebHook options.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddWebHooks<TSub>(
        this IServiceCollection serviceCollection,
        Action<IWebHookOptionBuilder<TSub>>? builder = null
    )
        where TSub : WebHookSubscription
    {
        var configuration = new WebHookOptionBuilder<TSub>();
        builder?.Invoke(configuration);

        ValidateConfiguration(configuration);
        RegisterServices(serviceCollection, configuration);
        ConfigurePolly(serviceCollection, configuration);
        ConfigureHangfire(configuration);
        ConfigureInterceptors(serviceCollection, configuration);

        return serviceCollection;
    }

    private static void ConfigureInterceptors<TSub>(
        IServiceCollection services,
        WebHookOptionBuilder<TSub> configuration
    )
        where TSub : WebHookSubscription
    {
        services.AddHostedService<WebHookInitializationHostedService>();

        foreach (var interceptorType in configuration.Interceptors)
        {
            services.AddKeyedTransient(
                typeof(IWebHookInterceptor<TSub>),
                interceptorType.Name,
                interceptorType
            );
        }
    }

    private static void RegisterServices<TSub>(
        IServiceCollection services,
        WebHookOptionBuilder<TSub> configuration
    )
        where TSub : WebHookSubscription
    {
        services.AddSingleton<IWebHookOptionBuilder<TSub>>(configuration);

        services.AddTransient<WebHookInterceptorAggregator<TSub>>();
        services.AddTransient<
            IAfterAllAttemptsFailedInterceptor,
            WebHookInterceptorAggregator<TSub>
        >();
        services.AddTransient<IWebHookManager<TSub>, WebHookManager<TSub>>();
        services.AddTransient<IWebHookEventPublisher, WebHookEventPublisher<TSub>>();
        services.AddTransient<WebHookProcessor<TSub>>();
        services.AddSingleton<IWebHookSignatureService<TSub>, WebHookHMACSignatureService<TSub>>();
        services.AddSingleton<
            IWebHookDeliveryJobFailureFilter,
            WebHookDeliveryJobFailureFilter<TSub>
        >();
    }

    private static void ConfigurePolly<TSub>(
        IServiceCollection services,
        WebHookOptionBuilder<TSub> configuration
    )
        where TSub : WebHookSubscription
    {
        services.AddResiliencePipeline(
            "default",
            b =>
            {
                b.AddRetry(
                        new RetryStrategyOptions
                        {
                            Delay = configuration.ResilienceOptions.Delay,
                            BackoffType = configuration.ResilienceOptions.BackoffType,
                            UseJitter = configuration.ResilienceOptions.UseJitter,
                            MaxRetryAttempts = configuration.ResilienceOptions.MaxRetryAttempts,
                            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                        }
                    )
                    .AddTimeout(configuration.ResilienceOptions.Timeout);
            }
        );
    }

    private static void ConfigureHangfire<TSub>(WebHookOptionBuilder<TSub> configuration)
        where TSub : WebHookSubscription
    {
        CustomRetryAttribute.SetDelayIntervals(configuration.HangfireDelayInMinutes);
    }

    private static void ValidateConfiguration<TSub>(WebHookOptionBuilder<TSub> configuration)
        where TSub : WebHookSubscription
    {
        if (
            configuration.UseSigning
            && string.IsNullOrWhiteSpace(configuration.SignatureEncryptionKey)
        )
        {
            throw new InvalidOperationException(
                "Signature encryption key must be provided when signing is enabled."
            );
        }
    }
}
