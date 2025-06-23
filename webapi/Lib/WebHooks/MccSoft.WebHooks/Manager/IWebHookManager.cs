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
    /// Creates and registers a new WebHook subscription in the system.
    ///
    /// This method persists a subscription of type <typeparamref name="TSub"/> in the database,
    /// encrypts and stores a secret used for signing webhook payloads, and returns
    /// the saved subscription along with the raw (unencrypted) secret.
    /// The raw secret should be shown to the user only once immediately after creation.
    ///
    /// The raw secret is not persisted in plaintext and cannot be retrieved again later.
    /// </summary>
    /// <param name="name">Human-readable name of the WebHook subscription.</param>
    /// <param name="url">Target callback URL to which the WebHook payloads will be sent.</param>
    /// <param name="eventType">Type of event that triggers this WebHook.</param>
    /// <param name="method">HTTP method to be used for the callback (e.g., POST or PUT).</param>
    /// <param name="headers">Optional custom headers to include when sending the WebHook request.</param>
    /// <returns>
    /// A <see cref="SubscriptionResult{TSub}"/> containing:
    /// <para>  - The created <typeparamref name="TSub"/> subscription entity.</para>
    /// <para>  - The raw secret (in plaintext) that must be stored by the client for future signature validation.</para>
    /// </returns>
    Task<SubscriptionResult<TSub>> Subscribe(
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
    /// Re-generates and updates the signing secret for the given WebHook subscription.
    ///
    /// This operation replaces the existing signing secret with a newly generated one,
    /// which will be used for signing all future webhook payloads for this subscription.
    ///
    /// The new secret is returned in plaintext only once and must be securely stored by the client.
    /// It is not persisted in plaintext and cannot be retrieved later.
    /// </summary>
    /// <remarks>
    /// Previously sent or retried events (before the rotation) will still be signed using the old secret.
    /// Only newly triggered events will be signed with the newly generated secret.
    /// </remarks>
    /// <param name="subscriptionId">The unique identifier of the WebHook subscription.</param>
    /// <returns>The new raw (decrypted) secret that should be provided to the target service.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if signing is disabled or encryption configuration is missing,
    /// or if the specified subscription could not be found.
    /// </exception>
    Task<string> RotateSecret(Guid subscriptionId);
}
