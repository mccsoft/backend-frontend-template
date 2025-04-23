using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MccSoft.WebHooks;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class WebHookEventPublisher<TSub> : IWebHookEventPublisher
    where TSub : WebHookSubscription
{
    private readonly DbContext _dbContext;
    private readonly IBackgroundJobClient _jobClient;

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
}
