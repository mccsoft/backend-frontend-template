using System;
using System.Collections.Generic;

namespace MccSoft.WebHooks.Domain;

public class WebHookSubscription
{
    /// <summary>
    /// Subscription Id.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Webhook name (Human readable name of integration).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Webhook URL for integration.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// HTTP method for accessing URL via specified http method.
    /// </summary>
    public string Method { get; set; }

    public string EventType { get; set; }

    public Dictionary<string, string> Headers { get; set; } = [];

    public DateTime SubscribedAt { get; init; }

    /// <summary>
    /// Default EF core constructor
    /// </summary>
    protected WebHookSubscription() { }

    /// <summary>
    /// Constructor for creating subscription.
    /// </summary>
    /// <param name="name">Human-readable name for integration.</param>
    /// <param name="url">URL for integration (callback)</param>
    /// <param name="eventType"></param>
    /// <param name="method">Optional HTTP method</param>
    /// <param name="headers"></param>
    public WebHookSubscription(
        string name,
        string url,
        string eventType,
        string method,
        Dictionary<string, string>? headers = null
    )
    {
        Name = name;
        Url = url;
        EventType = eventType;
        Method = method;
        Headers = headers ?? [];

        SubscribedAt = DateTime.UtcNow;
    }

    public WebHook<TSub> CreateWebHook<TSub>(string data)
        where TSub : WebHookSubscription => new((TSub)this, EventType, data);
}
