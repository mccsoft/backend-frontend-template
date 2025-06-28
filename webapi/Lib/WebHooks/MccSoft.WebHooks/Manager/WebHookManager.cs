using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using MccSoft.WebHooks.Configuration;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Processing;
using MccSoft.WebHooks.Signing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MccSoft.WebHooks.Manager;

public class WebHookManager<TSub> : IWebHookManager<TSub>
    where TSub : WebHookSubscription
{
    private readonly ILogger<WebHookManager<TSub>> _logger;
    private readonly DbContext _dbContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IWebHookSignatureService<TSub> _signatureService;

    public WebHookManager(
        IServiceScopeFactory serviceScopeFactory,
        IServiceProvider serviceProvider,
        ILogger<WebHookManager<TSub>> logger,
        IWebHookSignatureService<TSub> signatureService
    )
    {
        if (WebHookRegistration.DbContextType == null)
            throw new ArgumentNullException(
                $"WebHookRegistration.DbContextType wasn't initialized. Did you call builder.AddWebHookEntities() from DbContext OnModelCreating?"
            );

        _serviceScopeFactory = serviceScopeFactory;
        _dbContext = (DbContext)
            serviceProvider.GetRequiredService(WebHookRegistration.DbContextType);

        _logger = logger;
        _signatureService = signatureService;
    }

    /// <inheritdoc />
    public async Task<TSub> GetSubscriptionAsync(Guid subscriptionId)
    {
        return await _dbContext
            .WebHookSubscriptions<TSub>()
            .FirstAsync(x => x.Id == subscriptionId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TSub>> GetSubscriptionsAsync(bool withAttempts = false)
    {
        return await _dbContext.WebHookSubscriptions<TSub>().ToListAsync();
    }

    /// <inheritdoc />
    public async Task<SubscriptionResult<TSub>> Subscribe(
        string name,
        string url,
        string eventType,
        HttpMethod? method = null,
        Dictionary<string, string>? headers = null
    )
    {
        var subscription = (TSub)
            Activator.CreateInstance(typeof(TSub), [name, url, eventType, method, headers]);

        var decryptedSecret = _signatureService.GenerateEncryptedSecret(subscription);

        _dbContext.WebHookSubscriptions<TSub>().Add(subscription);
        await _dbContext.SaveChangesAsync();

        return new SubscriptionResult<TSub>(subscription, decryptedSecret);
    }

    /// <inheritdoc />
    public async Task Unsubscribe(Guid subscriptionId)
    {
        // We use local context because we need to hold context during all method
        using var scope = _serviceScopeFactory.CreateScope();
        var provider = scope.ServiceProvider;
        var localDbContext = (DbContext)
            provider.GetRequiredService(WebHookRegistration.DbContextType);

        var sub = await localDbContext
            .WebHookSubscriptions<TSub>()
            .Where(x => x.Id == subscriptionId)
            .FirstOrDefaultAsync();

        if (sub is null)
            return;

        var webHookIdsToDelete = await localDbContext
            .WebHooks<TSub>()
            .Where(x => x.SubscriptionId == subscriptionId)
            .Select(x => x.Id)
            .ToListAsync();

        if (webHookIdsToDelete.Count > 0)
            DeleteRunningJobs(webHookIdsToDelete);

        localDbContext.Remove(sub);
        await localDbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<string> RotateSecret(Guid subscriptionId)
    {
        var webHookSubscription = await _dbContext
            .WebHookSubscriptions<TSub>()
            .FirstAsync(webhook => webhook.Id == subscriptionId);

        var decryptedSecret = _signatureService.GenerateEncryptedSecret(webHookSubscription);
        if (string.IsNullOrWhiteSpace(decryptedSecret))
            throw new InvalidOperationException(
                $"{nameof(IWebHookOptionBuilder<TSub>.UseSigning)} is disabled or EncryptionKey is missing."
            );

        await _dbContext.SaveChangesAsync();
        return decryptedSecret;
    }

    /// <inheritdoc />
    public async Task<TSub> UpdateSubscriptionAsync(TSub subscription)
    {
        _dbContext.Update(subscription);
        await _dbContext.SaveChangesAsync();
        return subscription;
    }

    private void DeleteRunningJobs(List<int> webHookIdsToDelete)
    {
        var mon = JobStorage.Current.GetMonitoringApi();
        var scheduledJobsKeys = mon.ScheduledJobs(0, int.MaxValue)
            .Where(o => FilterJobs(o.Value.Job, webHookIdsToDelete))
            .Select(x => x.Key)
            .ToList();

        var processingJobsKeys = mon.ProcessingJobs(0, int.MaxValue)
            .Where(o => FilterJobs(o.Value.Job, webHookIdsToDelete))
            .Select(x => x.Key)
            .ToList();

        var enqueuedJobsKeys = mon.Queues()
            .SelectMany(x => mon.EnqueuedJobs(x.Name, 0, int.MaxValue))
            .Where(o => FilterJobs(o.Value.Job, webHookIdsToDelete))
            .Select(x => x.Key)
            .ToList();

        var allJobKeys = scheduledJobsKeys
            .Concat(processingJobsKeys)
            .Concat(enqueuedJobsKeys)
            .ToList();

        if (allJobKeys is { })
            allJobKeys.ForEach(x => BackgroundJob.Delete(x));
    }

    private bool FilterJobs(Job job, List<int> webHookIdsToDelete) =>
        job.Method.Name == nameof(WebHookProcessor<TSub>.RunWebHookDeliveryJob)
        && webHookIdsToDelete.Contains((int)job.Args[0]);
}
