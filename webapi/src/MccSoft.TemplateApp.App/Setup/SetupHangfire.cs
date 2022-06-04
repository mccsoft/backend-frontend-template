using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.PostgreSql;

namespace MccSoft.TemplateApp.App.Setup;

public static class SetupHangfire
{
    public const string DisableHangfireOptionName = nameof(DisableHangfireOptionName);

    public static void AddHangfire(IServiceCollection services, string connectionString)
    {
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
    }

    public static void UseHangfire(WebApplication app)
    {
        if (app.Configuration.GetValue<bool>(DisableHangfireOptionName))
            return;

        // app.UseHangfireServer(new BackgroundJobServerOptions { WorkerCount = 2 });

        // var therapySyncJobSettings = Configuration.GetSection(nameof(TherapyDataSyncJobSettings))
        //     .Get<HangFireJobSettings>();
        // RecurringJob.AddOrUpdate<TherapyDataSyncJob>(
        //     nameof(TherapyDataSyncJob),
        //     job => job.Execute(),
        //     therapySyncJobSettings.CronExpression);

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
}
