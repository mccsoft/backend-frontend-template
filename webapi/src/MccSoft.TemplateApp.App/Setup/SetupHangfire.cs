using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.PostgreSql;
using Hangfire.Storage;
using MccSoft.WebApi.Jobs;

namespace MccSoft.TemplateApp.App.Setup;

public static partial class SetupHangfire
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
        services.RegisterJobs();
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

        ConfigureJobs(app);
    }

    static partial void RegisterJobs(this IServiceCollection services);

    static partial void ConfigureJobs(WebApplication app);

    /// <summary>
    /// Removes all existing Hangfire Jobs.
    /// It makes sense to remove all jobs before adding new ones.
    /// This way if you added a Job and later removed it in newer version, you won't have to remember the job names and remove them manually.
    /// See https://stackoverflow.com/questions/40277390/how-to-remove-all-hangfire-recurring-jobs-on-startup for details
    /// </summary>
    private static void RemoveAllJobs()
    {
        using var connection = JobStorage.Current.GetConnection();
        foreach (var recurringJob in connection.GetRecurringJobs())
        {
            RecurringJob.RemoveIfExists(recurringJob.Id);
        }
    }

    private static void ConfigureJob<TJob, TOptions>(
        this IConfiguration appConfiguration,
        IRecurringJobManager recurringJobManager
    )
        where TJob : JobBase
        where TOptions : HangFireJobSettings
    {
        var jobSettings = Activator.CreateInstance<TOptions>();
        appConfiguration.GetSection(jobSettings.GetType().Name).Bind(jobSettings);

        if (jobSettings.IsEnabled != false)
            recurringJobManager.AddOrUpdate<TJob>(jobSettings.CronExpression);
    }

    public static void AddOrUpdate<TJob>(
        this IRecurringJobManager recurringJobManager,
        string cronExpression
    )
        where TJob : JobBase
    {
        recurringJobManager.AddOrUpdate<TJob>(
            typeof(TJob).Name,
            job => job.Execute(),
            cronExpression
        );
    }
}
