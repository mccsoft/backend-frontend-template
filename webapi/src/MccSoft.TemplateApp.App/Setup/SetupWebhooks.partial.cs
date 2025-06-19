using MccSoft.TemplateApp.Domain.WebHook;
using MccSoft.WebHooks;
using Polly;

namespace MccSoft.TemplateApp.App.Setup;

public partial class SetupWebhooks
{
    static partial void AddProjectSpecifics(
        WebApplicationBuilder builder,
        WebHookOptionBuilder<TemplateWebHookSubscription> optionsBuilder
    )
    {
        ConfigureResilienceOptions(optionsBuilder);
        ConfigureInterceptors(builder, optionsBuilder);
    }

    private static void ConfigureResilienceOptions(
        WebHookOptionBuilder<TemplateWebHookSubscription> optionsBuilder
    )
    {
        optionsBuilder.ResilienceOptions.Delay = TimeSpan.FromSeconds(2);
        optionsBuilder.ResilienceOptions.BackoffType = DelayBackoffType.Exponential;
        optionsBuilder.ResilienceOptions.UseJitter = true;
        optionsBuilder.ResilienceOptions.MaxRetryAttempts = 5;
        optionsBuilder.ResilienceOptions.Timeout = TimeSpan.FromSeconds(30);

        optionsBuilder.UseSigning = true;
    }

    private static void ConfigureInterceptors(
        WebApplicationBuilder builder,
        WebHookOptionBuilder<TemplateWebHookSubscription> optionsBuilder
    )
    {
        using ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<SetupWebhooks>>();
        optionsBuilder.WebHookInterceptors.BeforeExecution = (webHook) =>
        {
            logger.LogInformation("Start delivering Webhook with ID: {WebHookId}", webHook?.Id);
        };
        optionsBuilder.WebHookInterceptors.AfterAllAttemptsFailed = (webHookId) =>
        {
            logger.LogInformation($"Webhook with ID: {webHookId} failed");
        };
        optionsBuilder.WebHookInterceptors.ExecutionSucceeded = (webHook) =>
        {
            logger.LogInformation(
                "Webhook with ID: Webhook with ID: {WebHookId} delivered",
                webHook?.Id
            );
        };
    }
}
