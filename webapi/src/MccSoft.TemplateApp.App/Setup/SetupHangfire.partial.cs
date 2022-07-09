using Hangfire;
using MccSoft.TemplateApp.App.Jobs;
using MccSoft.TemplateApp.App.Settings;

namespace MccSoft.TemplateApp.App.Setup;

public static partial class SetupHangfire
{
    static partial void ConfigureJobs(WebApplication app)
    {
        var appConfiguration = app.Configuration;
        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

        // Configure your jobs here: Pass a Job and its settings inherited from HangFire Job Settings
        appConfiguration.ConfigureJob<ProductDataLoggerJob, ProductDataLoggerJobSettings>(
            recurringJobManager
        );
    }

    static partial void RegisterJobs(this IServiceCollection services)
    {
        // Add your custom JobServices for resolving them from DI.
        services.AddScoped<ProductDataLoggerJob>();
    }
}
