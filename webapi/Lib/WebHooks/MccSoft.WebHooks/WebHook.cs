using System;
using System.Net.Http;
using System.Text.Json;

public class WebHook
{
    public int Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastRun { get; private set; }
    public DateTime? NextRun { get; private set; }
    public bool IsSucceded { get; private set; }

    ///<summary>
    /// Response received from a webhook
    ///</summary>
    public string? LastError { get; private set; }

    ///<summary>
    /// Response received from a webhook
    ///</summary>
    public string Response { get; private set; }

    ///<summary>
    /// Number of attempts performed
    ///</summary>
    public int AttemptsPerformed { get; private set; }

    public string TargetUrl { get; private set; }
    public HttpMethod HttpMethod { get; private set; }
    public string SerializedBody { get; private set; }

    public void MarkSuccessful(string webHookResponse)
    {
        Response = webHookResponse;

        NextRun = null;
        LastError = null;
        IsSucceded = true;
    }

    public void MarkFailed(Exception e)
    {
        LastError = JsonSerializer.Serialize(e);
        IsSucceded = false;
    }

    internal void BeforeRun(DateTime now, int delayTillNextExecution)
    {
        LastRun = now;
        NextRun = now.AddMinutes(delayTillNextExecution);
        AttemptsPerformed++;
    }
}
