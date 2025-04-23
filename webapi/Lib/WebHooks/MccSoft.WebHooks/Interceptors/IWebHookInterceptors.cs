using System;
using MccSoft.WebHooks.Domain;

namespace MccSoft.WebHooks.Interceptors;

public record AfterAllAttemptsFailedArgs<TSub>(WebHook<TSub> WebHook, Exception Exception)
    where TSub : WebHookSubscription;

public interface IWebHookInterceptors<TSub>
    where TSub : WebHookSubscription
{
    Action<int>? AfterAllAttemptsFailed { get; set; }

    Action<int>? ExecutionSucceeded { get; set; }

    Action<int>? BeforeExecution { get; set; }
}
