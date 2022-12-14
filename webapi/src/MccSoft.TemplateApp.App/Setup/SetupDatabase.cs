using MccSoft.NpgSql;
using MccSoft.PersistenceHelpers;
using MccSoft.PersistenceHelpers.DomainEvents;
using MccSoft.TemplateApp.Persistence;
using Microsoft.EntityFrameworkCore;
using NeinLinq;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
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

        await DoRunMigration(scope.ServiceProvider);
    }

    public static async Task DoRunMigration(IServiceProvider serviceProvider)
    {
        await using var context = serviceProvider.GetRequiredService<TemplateAppDbContext>();
        await context.Database.MigrateAsync();

        await serviceProvider.SeedOpenIdClientsAsync();

        context.ReloadTypesForEnumSupport();

        // If you'd like to modify this class, consider adding your custom code in the SetupDatabase.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        await SeedDatabase(serviceProvider, context);
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
                    opt.UseNpgsql(
                            connectionString,
                            builder => builder.EnableRetryOnFailureWithAdditionalErrorCodes()
                        )
                        .WithLambdaInjection()
                        .AddDomainEventsInterceptors(provider);
                    opt.UseOpenIddict();
                }
            );

        services
            .AddScoped<Func<TemplateAppDbContext>>(provider => () => CreateDbContext(provider))
            .RegisterRetryHelper();

        SetupHangfire.AddHangfire(services, connectionString, configuration);

        // If you'd like to modify this class, consider adding your custom code in the SetupDatabase.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        AddProjectSpecifics(builder);
        AddSeeders(services, configuration);
    }

    private static partial TemplateAppDbContext CreateDbContext(IServiceProvider provider);

    /// <summary>
    /// Performs database seeding operation.
    /// It's public because it's also called from ComponentTests
    /// </summary>
    /// <returns></returns>
    public static partial Task SeedDatabase(
        IServiceProvider serviceProvider,
        TemplateAppDbContext context
    );

    static partial void AddSeeders(IServiceCollection services, IConfiguration configuration);

    static partial void AddProjectSpecifics(WebApplicationBuilder builder);
}
