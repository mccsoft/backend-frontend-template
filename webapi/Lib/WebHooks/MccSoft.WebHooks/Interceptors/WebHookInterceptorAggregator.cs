using System;
using System.Collections.Generic;
using MccSoft.WebHooks.Domain;
using Microsoft.Extensions.Logging;

namespace MccSoft.WebHooks.Interceptors;

public class WebHookInterceptorAggregator<TSub> : IWebHookInterceptor<TSub>
    where TSub : WebHookSubscription
{
    private readonly IEnumerable<IWebHookInterceptor<TSub>> _interceptors;
    private readonly ILogger<WebHookInterceptorAggregator<TSub>> _logger;

    public WebHookInterceptorAggregator(
        IEnumerable<IWebHookInterceptor<TSub>> interceptors,
        ILogger<WebHookInterceptorAggregator<TSub>> logger
    )
    {
        _interceptors = interceptors;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void AfterAllAttemptsFailed(int webHookId)
    {
        foreach (var webHookInterceptor in _interceptors)
        {
            try
            {
                webHookInterceptor.AfterAllAttemptsFailed(webHookId);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    $"Error invoking interceptor AfterAllAttemptsFailed {webHookInterceptor.GetType().Name}, webHookId {webHookId}"
                );
            }
        }
    }

    /// <inheritdoc/>
    public void ExecutionSucceeded(WebHook<TSub> webHook)
    {
        foreach (var webHookInterceptor in _interceptors)
        {
            try
            {
                webHookInterceptor.ExecutionSucceeded(webHook);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    $"Error invoking interceptor ExecutionSucceeded {webHookInterceptor.GetType().Name}, webHookId {webHook?.Id}"
                );
            }
        }
    }

    /// <inheritdoc/>
    public void BeforeExecution(WebHook<TSub> webHook)
    {
        foreach (var webHookInterceptor in _interceptors)
        {
            try
            {
                webHookInterceptor.BeforeExecution(webHook);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    $"Error invoking interceptor BeforeExecution {webHookInterceptor.GetType().Name}, webHookId {webHook?.Id}"
                );
            }
        }
    }
}
