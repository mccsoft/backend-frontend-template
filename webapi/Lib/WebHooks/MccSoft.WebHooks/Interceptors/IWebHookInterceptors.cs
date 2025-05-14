using System;
using MccSoft.WebHooks.Domain;

namespace MccSoft.WebHooks.Interceptors;

public record AfterAllAttemptsFailedArgs<TSub>(WebHook<TSub> WebHook, Exception Exception)
    where TSub : WebHookSubscription;

public interface IWebHookInterceptors<TSub>
    where TSub : WebHookSubscription
{
    /// <summary>
    /// Callback invoked when all retry attempts for a WebHook have been exhausted and it still fails.
    /// Provides the WebHook ID for logging or additional custom handling (e.g. alerting, cleanup).
    /// </summary>
    Action<int>? AfterAllAttemptsFailed { get; set; }

    /// <summary>
    /// Callback invoked after a WebHook is successfully delivered.
    /// Provides the WebHook ID so that consumers can log, audit, or trigger follow-up actions.
    /// </summary>
    Action<int>? ExecutionSucceeded { get; set; }

    /// <summary>
    /// Callback invoked immediately before a WebHook is sent.
    /// Provides the WebHook ID to support custom pre-processing or logging.
    /// </summary>
    Action<int>? BeforeExecution { get; set; }
}
