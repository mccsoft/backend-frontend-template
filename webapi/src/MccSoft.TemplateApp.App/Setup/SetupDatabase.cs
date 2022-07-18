using MccSoft.LowLevelPrimitives;
using MccSoft.NpgSql;
using MccSoft.PersistenceHelpers.DomainEvents;
using MccSoft.TemplateApp.App.Services.Authentication;
using MccSoft.TemplateApp.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NeinLinq;
using Shaddix.OpenIddict.ExternalAuthentication.Infrastructure;

namespace MccSoft.TemplateApp.App.Setup;

public static partial class SetupDatabase
{
    public const string DisableMigrationOptionName = nameof(DisableMigrationOptionName);

    public static async Task RunMigration(WebApplication app)
    {
        if (app.Configuration.GetValue<bool>(DisableMigrationOptionName))
            return;

        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TemplateAppDbContext>();
        await context.Database.MigrateAsync();

        await app.SeedOpenIdClientsAsync();

        context.ReloadTypesForEnumSupport();

        // If you'd like to modify this class, consider adding your custom code in the SetupDatabase.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        await RunMigrationsProjectSpecific(app, scope, context);
    }

    public static void AddDatabase(WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        IConfiguration configuration = builder.Configuration;

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // this is needed for OpenIdDict and must go before .UseOpenIddict()
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = null;
        });

        services
            .AddEntityFrameworkNpgsql()
            .AddDbContext<TemplateAppDbContext>(
                (provider, opt) =>
                {
                    opt.UseNpgsql(connectionString, builder => builder.EnableRetryOnFailure())
                        .WithLambdaInjection()
                        .AddDomainEventsInterceptors(provider);
                    opt.UseOpenIddict();
                },
                contextLifetime: ServiceLifetime.Scoped,
                optionsLifetime: ServiceLifetime.Singleton
            );

        services
            .AddSingleton<Func<TemplateAppDbContext>>(provider => () => CreateDbContext(provider))
            .RegisterRetryHelper();

        SetupHangfire.AddHangfire(services, connectionString, configuration);

        // If you'd like to modify this class, consider adding your custom code in the SetupDatabase.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        AddProjectSpecifics(builder);
        AddSeeders(services, configuration);
    }

    private static partial TemplateAppDbContext CreateDbContext(IServiceProvider provider);

    private static partial Task RunMigrationsProjectSpecific(
        WebApplication app,
        IServiceScope scope,
        TemplateAppDbContext context
    );

    static partial void AddSeeders(IServiceCollection services, IConfiguration configuration);

    static partial void AddProjectSpecifics(WebApplicationBuilder builder);
}
