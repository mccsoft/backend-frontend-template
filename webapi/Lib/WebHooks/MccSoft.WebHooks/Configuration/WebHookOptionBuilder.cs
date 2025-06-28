using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Interceptors;
using Polly;

namespace MccSoft.WebHooks.Configuration;

public class WebHookOptionBuilder<TSub> : IWebHookOptionBuilder<TSub>
    where TSub : WebHookSubscription
{
    /// <inheritdoc />
    public ResiliencePipelineOptions ResilienceOptions { get; set; } =
        new()
        {
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            MaxRetryAttempts = 5,
            Timeout = TimeSpan.FromSeconds(30),
        };

    /// <inheritdoc />
    public IWebHookInterceptors<TSub>? WebHookInterceptors { get; set; } =
        new WebHookInterceptors<TSub>();

    /// <inheritdoc />
    public IEnumerable<int> HangfireDelayInMinutes { get; set; } = [60];

    /// <inheritdoc />
    public string WebHookSignatureHeaderName { get; set; } = "X-Signature";

    /// <inheritdoc />
    public string SignatureEncryptionKey { get; set; } =
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    /// <inheritdoc />
    public bool UseSigning { get; set; } = true;

    #region Fluent APi configurations

    /// <inheritdoc />
    public IWebHookOptionBuilder<TSub> WithSigning(
        string encryptionKey,
        string? signatureHeader = null
    )
    {
        UseSigning = true;
        SignatureEncryptionKey = encryptionKey;
        if (!string.IsNullOrWhiteSpace(signatureHeader))
            WebHookSignatureHeaderName = signatureHeader;

        return this;
    }

    /// <inheritdoc />
    public IWebHookOptionBuilder<TSub> WithHangfireDelays(IEnumerable<int> delays)
    {
        HangfireDelayInMinutes = delays;
        return this;
    }

    #endregion
}
