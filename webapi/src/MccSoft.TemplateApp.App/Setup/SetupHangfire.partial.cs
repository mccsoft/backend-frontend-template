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

        // It makes sense to remove all jobs before adding one.
        // This way if you added a Job and later removed it in newer version, you won't have to remember the job names and remove them manually
        RemoveAllJobs();

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
