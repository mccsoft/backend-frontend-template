using System.Collections.Generic;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Interceptors;

namespace MccSoft.WebHooks.Configuration;

/// <summary>
/// Provides configuration options for WebHook processing,
/// including retry policies, interceptors, and Hangfire-specific settings.
/// </summary>
public interface IWebHookOptionBuilder<TSub>
    where TSub : WebHookSubscription
{
    /// <summary>Polly retry and timeout options.</summary>
    ResiliencePipelineOptions ResilienceOptions { get; set; }

    /// <summary>Ability to add custom logic for webhook lifecycle (e.g., logging, filtering).</summary>
    IWebHookOptionBuilder<TSub> AddInterceptor<TInterceptor>()
        where TInterceptor : IWebHookInterceptor<TSub>;

    /// <summary>Delays for Hangfire job retries (in minutes).</summary>
    IEnumerable<int> HangfireDelayInMinutes { get; set; }

    /// <summary>Name of the HTTP header for the signature.</summary>
    string WebHookSignatureHeaderName { get; set; }

    /// <summary>Base64 key used for encrypting secrets in DB.</summary>
    string SignatureEncryptionKey { get; set; }

    /// <summary>Enables/disables signing for webhook requests.</summary>
    /// <remarks>SignatureEncryptionKey must be defined if <c>TRUE</c>!</remarks>
    /// <seealso cref="SignatureEncryptionKey"/>
    bool UseSigning { get; set; }

    /// <summary>
    /// Enables signing for WebHooks and sets encryption key and optional custom header name.
    /// </summary>
    /// <param name="encryptionKey">Base64-encoded key used to encrypt secrets in DB.</param>
    /// <param name="signatureHeader">Optional HTTP header name for signature. Defaults to 'X-Signature'.</param>
    /// <returns>Fluent builder for chaining.</returns>
    IWebHookOptionBuilder<TSub> WithSigning(string encryptionKey, string? signatureHeader = null);

    /// <summary>
    /// Sets custom retry delay intervals for Hangfire job retries (in minutes).
    /// </summary>
    /// <param name="delays">
    /// A collection of delay durations in minutes. Each value defines the delay before the corresponding retry attempt.
    /// For example: <c>[1, 5, 15]</c> means 1st retry after 1 min, 2nd after 5 mins, etc.
    /// </param>
    /// <returns>The same builder instance to support fluent configuration.</returns>
    IWebHookOptionBuilder<TSub> WithHangfireDelays(IEnumerable<int> delays);
}
