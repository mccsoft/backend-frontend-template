namespace MccSoft.WebHooks.Manager;

/// <summary>
/// Represents the result of a webhook subscription operation,
/// including just created subscription and the plain-text secret (decrypted)
/// that should be shown to the user(API) only once.
/// </summary>
/// <typeparam name="TSub">Type of the webhook subscription.</typeparam>
public class SubscriptionResult<TSub>
{
    /// <summary>
    /// The created webhook subscription entity, as persisted in the database.
    /// </summary>
    public TSub Subscription { get; init; }

    /// <summary>
    /// The plain-text secret used to sign webhook requests.
    /// This value should be stored securely on the client side, as it is not retrievable later.
    /// </summary>
    public string Secret { get; init; }

    public SubscriptionResult(TSub subscription, string secret)
    {
        Subscription = subscription;
        Secret = secret;
    }
}
