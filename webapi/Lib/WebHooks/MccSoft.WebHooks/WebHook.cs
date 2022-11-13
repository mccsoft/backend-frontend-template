using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

[Owned]
[Index("Int")]
[Index("String")]
public class WebHookAdditionalData
{
    /// <summary>
    /// Required for EF
    /// </summary>
    protected WebHookAdditionalData() { }

    public WebHookAdditionalData(
        int? @int = null,
        string? @string = null,
        JsonDocument? json = null
    )
    {
        Int = @int;
        String = @string;
        Json = json;
    }

    public int? Int { get; private set; }
    public string? String { get; private set; }
    public JsonDocument? Json { get; private set; }
}

[Index(nameof(WebHook.IsSucceded), nameof(WebHook.NextRun))]
public class WebHook
{
    protected WebHook() { }

    public WebHook(
        string httpMethod,
        string url,
        string? body,
        Dictionary<string, string> headers,
        WebHookAdditionalData? additionalData = null
    )
    {
        HttpMethod = httpMethod;
        TargetUrl = url;
        SerializedBody = body;
        Headers = headers;
        AdditionalData = additionalData ?? new WebHookAdditionalData();
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastRun { get; private set; }
    public DateTime? NextRun { get; private set; }
    public bool IsSucceded { get; private set; }

    /// <summary>
    /// No more retries will be performed
    /// </summary>
    public bool IsFinished { get; private set; }

    /// <summary>
    /// Response received from a failed webhook
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    /// Response received from a successful webhook
    /// </summary>
    public string? Response { get; private set; }

    /// <summary>
    /// Number of attempts performed
    /// </summary>
    public int AttemptsPerformed { get; private set; }

    public string TargetUrl { get; } = null!;
    public string HttpMethod { get; } = null!;
    public string? SerializedBody { get; }

    public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
    public WebHookAdditionalData AdditionalData { get; private set; } = new WebHookAdditionalData();

    public void MarkSuccessful(string webHookResponse)
    {
        Response = webHookResponse;

        NextRun = null;
        LastError = null;
        IsSucceded = true;
        IsFinished = true;
    }

    public void MarkFailed(Exception e)
    {
        LastError = JsonSerializer.Serialize(e);
        IsSucceded = false;
    }

    public void MarkFailedNoRetry()
    {
        IsSucceded = false;
        IsFinished = true;
    }

    internal void BeforeRun(DateTime now, DateTime nextRun)
    {
        LastRun = now;
        NextRun = nextRun;
        AttemptsPerformed++;
    }
}
