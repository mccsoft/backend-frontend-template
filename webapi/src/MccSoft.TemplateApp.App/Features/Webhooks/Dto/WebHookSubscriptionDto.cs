namespace MccSoft.TemplateApp.App.Features.Webhooks.Dto;

/// <summary>
/// Webhook subscription DTO
/// </summary>
/// <para name="Id">Subscription Id</para>
/// <param name="Name">Human-readable name of integration</param>
/// <param name="Url">URL for integration</param>
public record WebhookSubscriptionDto(
    Guid Id,
    string Name,
    string Url,
    WebHookEventType eventType,
    string? method = null,
    Dictionary<string, string>? headers = null
);
