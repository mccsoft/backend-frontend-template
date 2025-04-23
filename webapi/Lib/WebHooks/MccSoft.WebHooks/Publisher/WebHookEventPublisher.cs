using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.WebHooks.Publisher;

/// <summary>
/// Responsible for publishing WebHook events and scheduling their delivery using Hangfire.
/// </summary>
/// <typeparam name="TSub">The type of WebHook subscription.</typeparam>
public class WebHookEventPublisher<TSub> : IWebHookEventPublisher
    where TSub : WebHookSubscription
{
    /// <summary>
    /// Initializes the WebHook event publisher by resolving the necessary services.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve dependencies.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the WebHookRegistration.DbContextType was not initialized.
    /// </exception>
    public WebHookEventPublisher(IServiceProvider serviceProvider)
    {
        if (WebHookRegistration.DbContextType == null)
            throw new ArgumentNullException(
                $"WebHookRegistration.DbContextType wasn't initialized. Did you call builder.AddWebHookEntities() from DbContext OnModelCreating?"
            );

        _dbContext = (DbContext)
            serviceProvider.GetRequiredService(WebHookRegistration.DbContextType);

        _jobClient = serviceProvider.GetRequiredService<IBackgroundJobClient>();
    }

    /// <inheritdoc />
    public async Task PublishEvent<T>(
        string eventType,
        T data,
        CancellationToken cancellationToken = default
    )
    {
        var subs = await _dbContext
            .WebHookSubscriptions<TSub>()
            .Where(x => x.EventType == eventType)
            .ToListAsync(cancellationToken);

        var webHooks = subs.Select(x => x.CreateWebHook<TSub>(JsonSerializer.Serialize(data)))
            .ToList();

        await _dbContext.AddRangeAsync(webHooks, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var webHook in webHooks)
            _jobClient.Enqueue<WebHookProcessor<TSub>>(
                x => x.RunWebHookDeliveryJob(webHook.Id, cancellationToken)
            );
    }

    private readonly DbContext _dbContext;
    private readonly IBackgroundJobClient _jobClient;
}
