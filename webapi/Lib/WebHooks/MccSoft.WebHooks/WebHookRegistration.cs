using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Hangfire;
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
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
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
        Action<WebHookOptionBuilder<TSub>>? builder = null
    )
        where TSub : WebHookSubscription
    {
        var configuration = new WebHookOptionBuilder<TSub>();
        builder?.Invoke(configuration);
        serviceCollection.AddSingleton(configuration);

        if (configuration.WebHookInterceptors is { })
            serviceCollection.AddSingleton(configuration.WebHookInterceptors);

        serviceCollection.AddTransient<IWebHookManager<TSub>, WebHookManager<TSub>>();
        serviceCollection.AddTransient<IWebHookEventPublisher, WebHookEventPublisher<TSub>>();
        serviceCollection.AddTransient<WebHookProcessor<TSub>>();
        serviceCollection.AddSingleton<IWebHookSignatureService, WebHookHMACSignatureService>();

        serviceCollection.AddResiliencePipeline(
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

        GlobalJobFilters.Filters.Add(new WebHookDeliveryJobFailureFilter<TSub>(serviceCollection));
        CustomRetryAttribute.SetDelayIntervals(configuration.HangfireDelayInMinutes);

        return serviceCollection;
    }
}

/// <summary>
/// Provides configuration options for WebHook processing,
/// including retry policies, interceptors, and Hangfire-specific settings.
/// </summary>
public class WebHookOptionBuilder<TSub>
    where TSub : WebHookSubscription
{
    /// <summary>
    /// Options for configuring Polly-based retry and timeout resilience policies.
    /// </summary>
    public ResiliencePipelineOptions ResilienceOptions { get; set; } =
        new()
        {
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            MaxRetryAttempts = 5,
            Timeout = TimeSpan.FromSeconds(30),
        };

    /// <summary>
    /// Optional custom interceptors to handle WebHook execution lifecycle events.
    /// </summary>
    public IWebHookInterceptors<TSub>? WebHookInterceptors { get; set; } =
        new WebHookInterceptors<TSub>();

    /// <summary>
    /// Defines delay intervals (in minutes) used by Hangfire retry policies for failed jobs.
    /// </summary>
    public IEnumerable<int> HangfireDelayInMinutes = [60];

    /// <summary>
    /// The name of the HTTP header used to transmit the WebHook signature.
    /// Default is <c>X-Signature</c>.
    /// Change this if your receivers expect a custom header name.
    /// </summary>
    public string WebhookSignatureHeaderName { get; set; } = "X-Signature";
}
