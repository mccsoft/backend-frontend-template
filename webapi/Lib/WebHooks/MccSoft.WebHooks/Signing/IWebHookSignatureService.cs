namespace MccSoft.WebHooks.Signing;

/// <summary>
/// Computes HMAC-based signatures for webhook payloads.
/// </summary>
public interface IWebHookSignatureService
{
    /// <summary>
    /// Computes a Base64-encoded HMACSHA256 signature of the payload.
    /// </summary>
    /// <param name="payload">Request body (sent webhook event data)</param>
    /// <param name="secret">Shared secret</param>
    string ComputeSignature(string payload, string secret);

    /// <summary>
    /// Returns the name of the header where signature is sent.
    /// </summary>
    string GetSignatureHeaderName();

    /// <summary>
    /// Validates signature for webhook event.
    /// </summary>
    bool ValidateSignature(string body, string secret, string signature);
}
