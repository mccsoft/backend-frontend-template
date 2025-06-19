using MccSoft.TemplateApp.App.Features.Webhooks.Dto;
using MccSoft.TemplateApp.Domain.WebHook;
using MccSoft.WebApi.Patching;
using MccSoft.WebHooks.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.TemplateApp.App.Features.Webhooks;

[Authorize]
[ApiController]
[Route("api/webhooks")]
public class WebHookController : Controller
{
    private readonly IWebHookManager<TemplateWebHookSubscription> _webHookManager;

    public WebHookController(IWebHookManager<TemplateWebHookSubscription> webHookManager)
    {
        _webHookManager = webHookManager;
    }

    /// <summary>
    /// Returns list of configured Webhooks
    /// </summary>
    [HttpGet("subscriptions")]
    public async Task<List<WebhookSubscriptionDto>> GetSubscriptions()
    {
        var subscriptions = await _webHookManager.GetSubscriptionsAsync();
        return subscriptions.Select(x => x.ToSubscriptionDto()).ToList();
    }

    /// <summary>
    /// Subscribes to a new webhook event.
    /// </summary>
    /// <param name="dto">The subscription request containing event type and target URL.</param>
    /// <returns>The created webhook subscription.</returns>
    /// <response code="200">Subscription created successfully.</response>
    /// <response code="400">Invalid subscription data.</response>
    [HttpPost("subscriptions/subscribe")]
    public async Task<WebHookSubscribedDto> SubscribeToEvent([FromBody] CreateWebHookDto dto)
    {
        var result = await _webHookManager.Subscribe(
            dto.Name,
            dto.Url,
            dto.EventType.ToString(),
            new HttpMethod(dto.method),
            dto.Headers
        );

        return new WebHookSubscribedDto(
            result.Id,
            result.Name,
            result.Url,
            (WebHookEventType)Enum.Parse(typeof(WebHookEventType), result.EventType),
            result.SignatureSecret,
            result.Method.ToString(),
            result.Headers
        );
    }

    /// <summary>
    /// Updates an existing webhook subscription.
    /// </summary>
    /// <param name="id">The ID of the subscription to update.</param>
    /// <param name="dto">The updated subscription data.</param>
    /// <returns>The updated webhook subscription.</returns>
    /// <response code="200">Subscription updated successfully.</response>
    /// <response code="404">Subscription not found.</response>
    [HttpPatch("subscriptions/{id}")]
    public async Task<WebhookSubscriptionDto> UpdateSubscription(
        Guid id,
        [FromBody] UpdateWebHookSubscriptionDto dto
    )
    {
        var sub = await _webHookManager.GetSubscriptionAsync(id);

        if (dto.IsFieldPresent(nameof(dto.EventType)))
        {
            sub.EventType = dto.EventType.ToString();
        }

        sub.Update(dto);

        var result = await _webHookManager.UpdateSubscriptionAsync(sub);
        return result.ToSubscriptionDto();
    }

    /// <summary>
    /// Unsubscribes from a webhook event.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription to remove.</param>
    /// <returns>An HTTP 200 OK result.</returns>
    /// <response code="200">Unsubscribed successfully.</response>
    /// <response code="404">Subscription not found.</response>
    [HttpDelete("subscriptions/unsubscribe")]
    public IActionResult Unsubscribe(Guid subscriptionId)
    {
        _webHookManager.Unsubscribe(subscriptionId);
        return new OkResult();
    }

    /// <summary>
    /// Rotate already generated signature secret.
    /// </summary>
    /// <param name="id">Subscription Id</param>
    [HttpPost("subscriptions/{id}/rotate-secret")]
    public async Task<string> RotateSecret(Guid id)
    {
        return await _webHookManager.RotateSecret(id);
    }
}
