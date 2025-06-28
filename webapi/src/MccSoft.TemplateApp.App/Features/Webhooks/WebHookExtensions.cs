using System.Linq.Expressions;
using MccSoft.TemplateApp.App.Features.Webhooks.Dto;
using MccSoft.TemplateApp.Domain.WebHook;
using NeinLinq;

namespace MccSoft.TemplateApp.App.Features.Webhooks;

public static class WebHookExtensions
{
    [InjectLambda]
    public static WebhookSubscriptionDto ToSubscriptionDto(
        this TemplateWebHookSubscription record
    ) => ToSubscriptionDtoExprCompiled.Value(record);

    public static readonly Lazy<
        Func<TemplateWebHookSubscription, WebhookSubscriptionDto>
    > ToSubscriptionDtoExprCompiled = new(ToSubscriptionDtoExpr.Compile);

    public static Expression<
        Func<TemplateWebHookSubscription, WebhookSubscriptionDto>
    > ToSubscriptionDtoExpr =>
        record =>
            new WebhookSubscriptionDto(
                record.Id,
                record.Name,
                record.Url,
                (WebHookEventType)Enum.Parse(typeof(WebHookEventType), record.EventType),
                record.IsSignatureDefined(),
                record.Method.ToString(),
                record.Headers
            );
}
