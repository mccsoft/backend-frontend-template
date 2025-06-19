using System;
using System.Security.Cryptography;
using System.Text;

namespace MccSoft.WebHooks.Signing;

/// <summary>
/// Computes HMAC-based signatures for webhook payloads.
/// </summary>
public class WebHookHMACSignatureService : IWebHookSignatureService
{
    /// <inheritdoc />
    public string ComputeSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var bodyBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(bodyBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <inheritdoc />
    /// <remarks>
    /// It should be used on client side (incoming webhook).
    /// It mostly demonstrates how to verify and for test proposal.
    /// </remarks>
    public bool ValidateSignature(string body, string secret, string signature)
    {
        var computed = ComputeSignature(body, secret);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(signature)
        );
    }
}
