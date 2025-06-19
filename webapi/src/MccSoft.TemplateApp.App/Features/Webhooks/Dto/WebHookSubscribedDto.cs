namespace MccSoft.TemplateApp.App.Features.Webhooks.Dto;

/// <summary>
/// Webhook subscription DTO
/// </summary>
/// <para name="Id">Subscription Id</para>
/// <param name="Name">Human-readable name of integration</param>
/// <param name="Url">URL for integration</param>
/// <para name="signatureSecret">Decrypted signature secret. It is one-time display</para>
public record WebHookSubscribedDto(
    Guid Id,
    string Name,
    string Url,
    WebHookEventType eventType,
    bool isSignatureGenerated,
    string signatureSecret,
    string? method = null,
    Dictionary<string, string>? headers = null
) : WebhookSubscriptionDto(Id, Name, Url, eventType, isSignatureGenerated, method, headers);
