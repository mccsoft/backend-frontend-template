using MccSoft.TemplateApp.Domain.WebHook;
using MccSoft.WebHooks;

namespace MccSoft.TemplateApp.App.Setup;

public partial class SetupWebhooks
{
    public static void AddWebhooks(WebApplicationBuilder builder)
    {
        builder.Services.AddWebHooks<TemplateWebHookSubscription>(optionsBuilder =>
        {
            // Define sequence of reties with delay in minutes.
            // By default - it will retry once in a hour (60 minutes).
            optionsBuilder.HangfireDelayInMinutes = [60, 120, 120];

            // If you'd like to modify this class, consider adding your custom code in the SetupSwagger.partial.cs
            // This will make it easier to pull changes from Template when Template is updated
            // (actually this file will be overwritten by a file from template, which will make your changes disappear)
            AddProjectSpecifics(builder, optionsBuilder);
        });
    }

    static partial void AddProjectSpecifics(
        WebApplicationBuilder builder,
        WebHookOptionBuilder<TemplateWebHookSubscription> optionsBuilder
    );
}
