using System.Security.Cryptography;
using MccSoft.TemplateApp.Domain.WebHook;
using MccSoft.WebHooks;
using MccSoft.WebHooks.Configuration;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Interceptors;
using Polly;

namespace MccSoft.TemplateApp.App.Setup;

public partial class SetupWebhooks
{
    public static void AddWebhooks(WebApplicationBuilder builder)
    {
        builder.Services.AddWebHooks<TemplateAppWebHookSubscription>(optionsBuilder =>
        {
            // Define sequence of reties with delay in minutes.
            // By default - it will retry once in an hour (60 minutes).
            optionsBuilder.HangfireDelayInMinutes = [60, 120, 120];

            ConfigureResilienceOptions(optionsBuilder);

            optionsBuilder.AddInterceptor<
                LoggingWebHookInterceptor<TemplateAppWebHookSubscription>
            >();
        });
    }

    private static void ConfigureResilienceOptions(
        IWebHookOptionBuilder<TemplateAppWebHookSubscription> optionsBuilder
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
}
