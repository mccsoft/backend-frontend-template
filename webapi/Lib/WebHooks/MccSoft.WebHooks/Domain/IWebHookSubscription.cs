using System;
using System.Collections.Generic;
using System.Net.Http;

namespace MccSoft.WebHooks.Domain;

public interface IWebHookSubscription
{
    /// <summary>
    /// Subscription Id.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Webhook name (Human readable name of integration).
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Webhook URL for integration.
    /// </summary>
    public string Url { get; init; }

    /// <summary>
    /// HTTP method for accessing URL via specified http method.
    /// </summary>
    public HttpMethod Method { get; init; }

    /// <summary>
    /// The type of event has subscribed to
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// Additional HTTP headers
    /// </summary>
    public Dictionary<string, string> Headers { get; }

    /// <summary>
    /// Date and time the subscriber was added
    /// </summary>
    public DateTime SubscribedAt { get; init; }
}
