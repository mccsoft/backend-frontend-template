using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MccSoft.WebHooks.Domain;

namespace MccSoft.WebHooks.Manager;

public interface IWebHookManager<TSub>
    where TSub : WebHookSubscription
{
    /// <summary>
    /// Requests webhook subscriptions by Id
    /// </summary>
    Task<TSub> GetSubscriptionAsync(Guid subscriptionId);

    /// <summary>
    /// Requests all created webhook subscriptions in tenant.
    /// </summary>
    Task<IEnumerable<TSub>> GetSubscriptionsAsync(bool withAttempts = false);

    /// <summary>
    /// Creates new WebHook integration
    /// </summary>
    /// <param name="name">Webhook human-readable name</param>
    /// <param name="url">Callback URL</param>
    /// <param name="eventType">Type of webhook event (triggered in business logic)</param>
    /// <param name="method">Remote HTTP method for calling <see cref="url"/>(Callback URL)</param>
    /// <param name="headers">Provided header with sent webhook event.</param>
    Task<TSub> Subscribe(
        string name,
        string url,
        string eventType,
        HttpMethod? method = null,
        Dictionary<string, string>? headers = null
    );

    /// <summary>
    /// Updates WebHook integration
    /// </summary>
    /// <param name="subscription"></param>
    Task<TSub> UpdateSubscriptionAsync(TSub subscription);

    /// <summary>
    /// Remove subscription
    /// </summary>
    /// <param name="subscriptionId">Subscription Id</param>
    Task Unsubscribe(Guid subscriptionId);

    /// <summary>
    /// Re-generates signing secret for current subscription.
    /// </summary>
    /// <remarks>All re-tried events will be sent with old signature. New ones - with re-generated secret.</remarks>
    /// <param name="subscriptionId">Subscription id</param>
    /// <exception cref="InvalidOperationException">Webhook subscription not found</exception>
    Task<string> RotateSecret(Guid subscriptionId);
}
