using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace MccSoft.WebHooks.Domain;

/// <summary>
/// Represents a subscription to a specific WebHook event,
/// including the endpoint URL, HTTP method, and custom headers.
/// </summary>
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
    /// The target URL to which WebHooks should be delivered.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The HTTP method (e.g., POST, PUT) used to deliver WebHooks.
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// The event type this subscription is interested in (e.g., "user.created").
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// Optional custom HTTP headers to include in the WebHook request.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// The UTC timestamp when the subscription was created.
    /// </summary>
    public DateTime SubscribedAt { get; init; }

    /// <summary>
    /// Secret is used to sign outgoing WebHooks.
    /// </summary>
    public string SignatureSecret { get; private set; }

    /// <summary>
    /// Default constructor required by EF Core.
    /// </summary>
    protected WebHookSubscription() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebHookSubscription"/> class.
    /// </summary>
    /// <param name="name">Human-readable name of the integration.</param>
    /// <param name="url">The callback URL for WebHook delivery.</param>
    /// <param name="eventType">The type of event this subscription listens to.</param>
    /// <param name="method">The HTTP method to use for delivery (e.g., "POST").</param>
    /// <param name="headers">Optional custom headers for the HTTP request.</param>
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
        SignatureSecret = GenerateSecret();
    }

    /// <summary>
    /// Creates a new <see cref="WebHook{TSub}"/> instance using the current subscription.
    /// </summary>
    /// <typeparam name="TSub">The specific type of the subscription.</typeparam>
    /// <param name="data">The serialized payload to be sent with the WebHook.</param>
    /// <returns>A new WebHook instance ready to be persisted and processed.</returns>
    public WebHook<TSub> CreateWebHook<TSub>(string data)
        where TSub : WebHookSubscription => new((TSub)this, EventType, data);

    /// <summary>
    /// Generates crypto-random secret for outgoing webhooks.
    /// </summary>
    /// <returns></returns>
    private static string GenerateSecret() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
}
