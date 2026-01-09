using System;
using System.Linq;
using Hangfire.States;
using Hangfire.Storage;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.WebHooks.Processing;

public class WebHookDeliveryJobFailureFilter<TSub> : IWebHookDeliveryJobFailureFilter
    where TSub : WebHookSubscription
{
    private readonly IServiceProvider _serviceProvider;

    public WebHookDeliveryJobFailureFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is DeletedState failedState)
        {
            var webHookId =
                (int?)context.BackgroundJob.Job.Args[0]
                ?? throw new ArgumentNullException($"Background job argument is not a webhook id");

            MarkWebhookAsFinished(webHookId);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) { }

    private void MarkWebhookAsFinished(int webHookId)
    {
        if (WebHookRegistration.DbContextType == null)
            throw new ArgumentNullException(
                $"WebHookRegistration.DbContextType wasn't initialized. Did you call builder.AddWebHookEntities() from DbContext OnModelCreating?"
            );
        var dbContext = (DbContext)
            _serviceProvider.GetRequiredService(WebHookRegistration.DbContextType);

        var webHook = dbContext.WebHooks<TSub>().First(x => x.Id == webHookId);
        webHook.FinishAttempts();

        dbContext.Update(webHook);
        dbContext.SaveChanges();

        var afterAllAttemptsFailedInterceptor =
            _serviceProvider.GetRequiredService<IAfterAllAttemptsFailedInterceptor>();
        afterAllAttemptsFailedInterceptor.AfterAllAttemptsFailed(webHookId);
    }
}
