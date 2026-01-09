using System;
using MccSoft.WebHooks.Domain;

namespace MccSoft.WebHooks.Interceptors;

public record AfterAllAttemptsFailedArgs<TSub>(WebHook<TSub> WebHook, Exception Exception)
    where TSub : WebHookSubscription;

public interface IAfterAllAttemptsFailedInterceptor
{
    /// <summary>
    /// Callback invoked when all retry attempts for a WebHook have been exhausted and it still fails.
    /// Provides the WebHook ID for logging or additional custom handling (e.g. alerting, cleanup).
    /// </summary>
    void AfterAllAttemptsFailed(int webHookId);
}

public interface IWebHookInterceptor<TSub> : IAfterAllAttemptsFailedInterceptor
    where TSub : WebHookSubscription
{
    /// <summary>
    /// Callback invoked after a WebHook is successfully delivered.
    /// Provides the WebHook so that consumers can log, audit, or trigger follow-up actions.
    /// </summary>
    void ExecutionSucceeded(WebHook<TSub> webHook);

    /// <summary>
    /// Callback invoked immediately before a WebHook is sent.
    /// Provides the WebHook to support custom pre-processing or logging.
    /// </summary>
    void BeforeExecution(WebHook<TSub> webHook);
}
