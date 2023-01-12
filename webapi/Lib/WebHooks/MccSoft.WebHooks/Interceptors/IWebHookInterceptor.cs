using System;
using System.Net.Http;
using System.Threading.Tasks;

public interface IWebHookInterceptor
{
    Task BeforeExecutionAttempt(WebHook webHook, HttpClient httpClient, HttpRequestMessage message);
    Task ExecutionSucceeded(WebHook webHook);
    Task ExecutionFailed(WebHook webHook, Exception e);
    Task AfterAllAttemptsFailed(WebHook webHook);
}
