using System;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Interceptors;

public class WebHookInterceptors<TSub> : IWebHookInterceptors<TSub>
    where TSub : WebHookSubscription
{
    public Action<int>? AfterAllAttemptsFailed { get; set; }

    public Action<WebHook<TSub>>? ExecutionSucceeded { get; set; }

    public Action<WebHook<TSub>>? BeforeExecution { get; set; }
}
