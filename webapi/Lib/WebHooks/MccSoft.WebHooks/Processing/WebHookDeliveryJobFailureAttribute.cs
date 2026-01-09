using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using MccSoft.WebHooks.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.WebHooks.Processing;

public class WebHookDeliveryJobFailureAttribute : JobFilterAttribute, IApplyStateFilter
{
    private readonly IServiceProvider _serviceProvider;

    public WebHookDeliveryJobFailureAttribute(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        string methodName = context.BackgroundJob.Job.Method.Name;
        if (methodName != nameof(WebHookProcessor<WebHookSubscription>.RunWebHookDeliveryJob))
            return;

        _serviceProvider
            .CreateScope()
            .ServiceProvider.GetRequiredService<IWebHookDeliveryJobFailureFilter>()
            .OnStateApplied(context, transaction);
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        string methodName = context.BackgroundJob.Job.Method.Name;
        if (methodName != nameof(WebHookProcessor<WebHookSubscription>.RunWebHookDeliveryJob))
            return;

        _serviceProvider
            .CreateScope()
            .ServiceProvider.GetRequiredService<IWebHookDeliveryJobFailureFilter>()
            .OnStateUnapplied(context, transaction);
    }
}
