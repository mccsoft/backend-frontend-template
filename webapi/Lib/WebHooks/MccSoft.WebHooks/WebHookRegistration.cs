using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Hangfire;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Interceptors;
using MccSoft.WebHooks.Manager;
using MccSoft.WebHooks.Processing;
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

public class WebHookOptionBuilder<TSub>
    where TSub : WebHookSubscription
{
    public ResiliencePipelineOptions ResilienceOptions { get; set; } =
        new()
        {
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            MaxRetryAttempts = 5,
            Timeout = TimeSpan.FromSeconds(30),
        };

    public IWebHookInterceptors<TSub>? WebHookInterceptors { get; set; } =
        new WebHookInterceptors<TSub>();

    public IEnumerable<int> HangfireDelayInMinutes = [60];
}
