using System.Security.Cryptography;
using MccSoft.TemplateApp.Domain.WebHook;
using MccSoft.WebHooks.Configuration;
using Polly;

namespace MccSoft.TemplateApp.App.Setup;

public partial class SetupWebhooks
{
    static partial void AddProjectSpecifics(
        WebApplicationBuilder builder,
        IWebHookOptionBuilder<TemplateWebHookSubscription> optionsBuilder
    )
    {
        ConfigureResilienceOptions(optionsBuilder);
        ConfigureInterceptors(builder, optionsBuilder);
    }

    private static void ConfigureResilienceOptions(
        IWebHookOptionBuilder<TemplateWebHookSubscription> optionsBuilder
    )
    {
        optionsBuilder.ResilienceOptions.Delay = TimeSpan.FromSeconds(2);
        optionsBuilder.ResilienceOptions.BackoffType = DelayBackoffType.Exponential;
        optionsBuilder.ResilienceOptions.UseJitter = true;
        optionsBuilder.ResilienceOptions.MaxRetryAttempts = 5;
        optionsBuilder.ResilienceOptions.Timeout = TimeSpan.FromSeconds(30);

        // take key from appsettings.
        optionsBuilder.WithSigning(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    }

    private static void ConfigureInterceptors(
        WebApplicationBuilder builder,
        IWebHookOptionBuilder<TemplateWebHookSubscription> optionsBuilder
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
