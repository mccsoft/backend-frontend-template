using MccSoft.WebHooks.Domain;
using Microsoft.Extensions.Logging;

namespace MccSoft.WebHooks.Interceptors;

public class LoggingWebHookInterceptor<TSub> : IWebHookInterceptor<TSub>
    where TSub : WebHookSubscription
{
    private readonly ILogger<LoggingWebHookInterceptor<TSub>> _logger;

    public LoggingWebHookInterceptor(ILogger<LoggingWebHookInterceptor<TSub>> logger)
    {
        _logger = logger;
    }

    public void AfterAllAttemptsFailed(int webHookId)
    {
        _logger.LogInformation($"Webhook with ID: {webHookId} failed");
    }

    public void ExecutionSucceeded(WebHook<TSub> webHook)
    {
        _logger.LogInformation(
            "Webhook with ID: Webhook with ID: {WebHookId} delivered",
            webHook?.Id
        );
    }

    public void BeforeExecution(WebHook<TSub> webHook)
    {
        _logger.LogInformation("Start delivering Webhook with ID: {WebHookId}", webHook?.Id);
    }
}
