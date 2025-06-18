using System;
using System.Security.Cryptography;
using System.Text;

namespace MccSoft.WebHooks.Signing;

/// <inheritdoc cref="IWebHookSignatureService"/>
public class WebHookSignatureService : IWebHookSignatureService
{
    private const string _signatureHeader = "X-Signature";

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
    public string GetSignatureHeaderName() => _signatureHeader;

    /// <inheritdoc />
    public bool ValidateSignature(string body, string secret, string signature)
    {
        var computed = ComputeSignature(body, secret);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(signature)
        );
    }
}
