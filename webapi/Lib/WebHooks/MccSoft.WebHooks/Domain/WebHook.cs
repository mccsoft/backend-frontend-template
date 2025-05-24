using System;
using System.Net;

namespace MccSoft.WebHooks.Domain;

public class WebHook<TSub>
    where TSub : WebHookSubscription
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Date and time when the event was triggered
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// The result of attempt
    /// </summary>
    public bool IsSucceeded { get; private set; }

    /// <summary>
    /// No more retries will be performed
    /// </summary>
    public bool IsFinished { get; private set; }

    /// <summary>
    /// Status code received from a client
    /// </summary>
    public int? StatusCode { get; private set; }

    /// <summary>
    /// Response received from a failed webhook
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    /// Response received from a successful webhook
    /// </summary>
    public string? Response { get; private set; }

    /// <summary>
    /// The type of event that was triggered
    /// </summary>
    public string EventType { get; init; }

    /// <summary>
    /// Data for the triggered event
    /// </summary>
    public string Data { get; init; }

    /// <summary>
    /// Number of attempts performed
    /// </summary>
    public int AttemptsPerformed { get; private set; }

    /// <summary>
    /// Date of last attempt (http call)
    /// </summary>
    public DateTime? LastAttempt { get; private set; }

    /// <summary>
    /// Proxy entity for Webhook subscription.
    /// </summary>
    public TSub Subscription { get; private set; }
    public Guid SubscriptionId { get; private set; }

    public WebHook(TSub subscription, string eventType, string data)
    {
        EventType = eventType;
        Data = data;
        Subscription = subscription;
        SubscriptionId = subscription.Id;
    }

    public WebHook() { }

    internal void MarkSuccessful(HttpStatusCode? statusCode, string webHookResponse)
    {
        StatusCode = (int?)statusCode;
        Response = webHookResponse;
        LastError = null;
        IsSucceeded = true;

        FinishAttempts();
    }

    internal void MarkFailed(HttpStatusCode? statusCode, string? message)
    {
        StatusCode = (int?)statusCode;
        Response = null;
        LastError = message;
        IsSucceeded = false;
    }

    internal void FinishAttempts()
    {
        IsFinished = true;
    }

    internal void BeforeAttempt(DateTime now)
    {
        LastAttempt = now;
        AttemptsPerformed++;
    }

    internal void ResetAttempts()
    {
        IsFinished = false;
    }
}
