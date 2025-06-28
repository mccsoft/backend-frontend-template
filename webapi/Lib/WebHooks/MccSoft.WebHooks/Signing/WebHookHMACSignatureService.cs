using System;
using System.Security.Cryptography;
using System.Text;
using MccSoft.WebHooks.Configuration;
using MccSoft.WebHooks.Domain;

namespace MccSoft.WebHooks.Signing;

/// <summary>
/// Computes HMAC-based signatures for webhook payloads.
/// </summary>
public class WebHookHMACSignatureService<TSub> : IWebHookSignatureService<TSub>
    where TSub : WebHookSubscription
{
    private readonly byte[] _key;
    private readonly IWebHookOptionBuilder<TSub> _configuration;

    public WebHookHMACSignatureService(IWebHookOptionBuilder<TSub> configuration)
    {
        _configuration = configuration;

        var base64Key = _configuration.SignatureEncryptionKey;
        if (string.IsNullOrWhiteSpace(base64Key) && _configuration.UseSigning)
            throw new InvalidOperationException("Missing encryption key in webhook configuration");
        _key = Convert.FromBase64String(base64Key);
    }

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
    public string GenerateEncryptedSecret(TSub subscription)
    {
        if (!_configuration.UseSigning)
            return "";

        var rawSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var encryptedSecret = EncryptSecret(rawSecret);
        subscription.UpdateSignatureSecret(encryptedSecret);

        return rawSecret;
    }

    /// <inheritdoc />
    public string DecryptSecret(string cipherText)
    {
        if (!_configuration.UseSigning)
            return "";

        var fullCipher = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[16];
        var cipher = new byte[fullCipher.Length - 16];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(decrypted);
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

    /// <summary>
    /// Encrypts secret to store in secure way.
    /// </summary>
    /// <param name="plainText"></param>
    private string EncryptSecret(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }
}
