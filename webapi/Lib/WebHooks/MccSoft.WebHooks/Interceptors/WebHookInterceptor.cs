using System;
using System.Net.Http;
using System.Threading.Tasks;

public abstract class WebHookInterceptor : IWebHookInterceptor
{
    public Task AfterAllAttemptsFailed(WebHook webHook)
    {
        return Task.CompletedTask;
    }

    public Task BeforeExecutionAttempt(
        WebHook webHook,
        HttpClient httpClient,
        HttpRequestMessage message
    )
    {
        return Task.CompletedTask;
    }

    public Task ExecutionFailed(WebHook webHook, Exception e)
    {
        return Task.CompletedTask;
    }

    public Task ExecutionSucceeded(WebHook webHook)
    {
        return Task.CompletedTask;
    }
}
