namespace MccSoft.TemplateApp.App.Features.Webhooks.Dto;

/// <summary>
/// Webhook subscription DTO
/// </summary>
/// <param name="Name">Human-readable name of integration</param>
/// <param name="Url">URL for integration</param>
public record CreateWebHookDto(
    string Name,
    string Url,
    WebHookEventType EventType,
    string? method = null,
    Dictionary<string, string>? Headers = null
);
