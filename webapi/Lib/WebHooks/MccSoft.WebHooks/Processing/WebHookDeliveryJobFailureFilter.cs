using System;
using System.Linq;
using Hangfire.States;
using Hangfire.Storage;
using MccSoft.WebHooks;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Interceptors;
using MccSoft.WebHooks.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class WebHookDeliveryJobFailureFilter<TSub> : IApplyStateFilter
    where TSub : WebHookSubscription
{
    private readonly IWebHookInterceptors<TSub> _webHookInterceptors;
    private readonly IServiceCollection _serviceCollection;

    public WebHookDeliveryJobFailureFilter(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
        var serviceProvider = serviceCollection.BuildServiceProvider();
        _webHookInterceptors = serviceProvider.GetRequiredService<IWebHookInterceptors<TSub>>();
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        string methodName = context.BackgroundJob.Job.Method.Name;
        if (methodName == nameof(WebHookProcessor<TSub>.RunWebHookDeliveryJob))
        {
            if (context.NewState is DeletedState failedState)
            {
                var webHookId =
                    (int?)context.BackgroundJob.Job.Args[0]
                    ?? throw new ArgumentNullException(
                        $"Background job argument is not a webhook id"
                    );

                MarkWebhookAsFinished(webHookId);

                _webHookInterceptors.AfterAllAttemptsFailed?.Invoke(webHookId);
            }
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) { }

    private void MarkWebhookAsFinished(int webHookId)
    {
        var serviceProvider = _serviceCollection.BuildServiceProvider();

        if (WebHookRegistration.DbContextType == null)
            throw new ArgumentNullException(
                $"WebHookRegistration.DbContextType wasn't initialized. Did you call builder.AddWebHookEntities() from DbContext OnModelCreating?"
            );
        var _dbContext = (DbContext)
            serviceProvider.GetRequiredService(WebHookRegistration.DbContextType);

        var webHook = _dbContext.WebHooks<TSub>().First(x => x.Id == webHookId);
        webHook.FinishAttempts();

        _dbContext.Update(webHook);
        _dbContext.SaveChanges();
    }
}
