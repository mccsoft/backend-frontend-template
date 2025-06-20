using MccSoft.WebHooks.Domain;

namespace MccSoft.WebHooks.Signing;

/// <summary>
/// Computes signatures for webhook payloads.
/// </summary>
public interface IWebHookSignatureService<TSub>
    where TSub : WebHookSubscription
{
    /// <summary>
    /// Computes a Base64-encoded HMACSHA256 signature of the payload.
    /// </summary>
    /// <param name="payload">Request body (sent webhook event data)</param>
    /// <param name="secret">Shared secret</param>
    string ComputeSignature(string payload, string secret);

    /// <summary>
    /// Generates crypto-random secret for outgoing webhooks and sets encrypted value to provided subscription.
    /// </summary>
    /// <returns>Raw secret (decrypted) for one-time usage</returns>
    string GenerateEncryptedSecret(TSub subscription);

    /// <summary>
    /// Decrypts secret to use for signing outgoing webhooks.
    /// </summary>
    /// <param name="encrypted"></param>
    string DecryptSecret(string encrypted);

    /// <summary>
    /// Validates signature for webhook event.
    /// </summary>
    bool ValidateSignature(string body, string secret, string signature);
}
