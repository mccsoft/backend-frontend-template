using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.PostgreSql;
using MccSoft.TemplateApp.App.Jobs;
using MccSoft.TemplateApp.App.Settings;

namespace MccSoft.TemplateApp.App.Setup;

public static class SetupHangfire
{
    public static void AddHangfire(
        IServiceCollection services,
        string connectionString,
        IConfiguration configuration
    )
    {
        if (configuration.GetSection("Hangfire").GetValue<bool>("Disable"))
            return;

        services.AddHangfire(
            config =>
                config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(
                        connectionString,
                        new PostgreSqlStorageOptions()
                        {
                            DistributedLockTimeout = TimeSpan.FromSeconds(20),
                            PrepareSchemaIfNecessary = true,
                        }
                    )
        );
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 2;
        });
        services.RegisterHangfireJobs();
    }

    public static void UseHangfire(WebApplication app)
    {
        var appConfiguration = app.Configuration;
        var recurringJobManager = app.Services.GetService<IRecurringJobManager>();

        if (
            recurringJobManager == null
            || appConfiguration.GetSection("Hangfire").GetValue<bool>("Disable")
        )
            return;

        // Configure your jobs here: Pass a Job and its settings inherited from HangFire Job Settings
        appConfiguration.ConfigureJob<ProductDataLoggerJob, ProductDataLoggerJobSettings>(
            recurringJobManager
        );

        IConfigurationSection configurationSection = app.Configuration.GetSection("Hangfire");
        // In case you will need to debug/monitor tasks you can use Dashboard.
        if (configurationSection.GetValue<bool>("EnableDashboard"))
        {
            app.UseHangfireDashboard(
                options: new DashboardOptions()
                {
                    Authorization = new[]
                    {
                        new BasicAuthAuthorizationFilter(
                            new BasicAuthAuthorizationFilterOptions()
                            {
                                Users = new[]
                                {
                                    new BasicAuthAuthorizationUser()
                                    {
                                        Login = configurationSection.GetValue<string>(
                                            "DashboardUser"
                                        ),
                                        PasswordClear = configurationSection.GetValue<string>(
                                            "DashboardPassword"
                                        ),
                                    }
                                }
                            }
                        )
                    }
                }
            );
        }
    }

    private static void RegisterHangfireJobs(this IServiceCollection services)
    {
        // Add your custom JobServices for resolving them from DI.
        services.AddScoped<ProductDataLoggerJob>();
    }

    private static void ConfigureJob<T, TOptions>(
        this IConfiguration appConfiguration,
        IRecurringJobManager recurringJobManager
    )
        where T : JobBase
        where TOptions : HangFireJobSettings
    {
        var jobSettings = Activator.CreateInstance<TOptions>();
        appConfiguration.GetSection(jobSettings.GetType().Name).Bind(jobSettings);

        recurringJobManager.AddOrUpdate<T>(
            typeof(T).Name,
            job => job.Execute(),
            jobSettings.CronExpression
        );
    }
}
