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
    /// <param name="name"></param>
    /// <param name="url"></param>
    /// <param name="eventType"></param>
    /// <param name="method"></param>
    /// <param name="headers"></param>
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
}
