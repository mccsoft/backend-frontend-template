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

public static class SetupDatabase
{
    public const string DisableMigrationOptionName = nameof(DisableMigrationOptionName);

    public static async Task RunMigration(WebApplication app)
    {
        if (app.Configuration.GetValue<bool>(DisableMigrationOptionName))
            return;

        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TemplateAppDbContext>();
        context.Database.Migrate();

        context.ReloadTypesForEnumSupport();

        DefaultUserSeeder seeder = scope.ServiceProvider.GetRequiredService<DefaultUserSeeder>();
        DefaultUserOptions defaultUser = scope.ServiceProvider
            .GetRequiredService<IOptions<DefaultUserOptions>>()
            .Value;
        if (
            !string.IsNullOrEmpty(defaultUser.UserName)
            && !string.IsNullOrEmpty(defaultUser.Password)
        )
        {
            await seeder.SeedUser(defaultUser.UserName, defaultUser.Password);
        }

        string siteUrl = app.Configuration.GetSection("General").GetValue<string>("SiteUrl");
        app.UseOpenIdDictApplicationsFromConfiguration(options => options.SetPublicUrl(siteUrl));
    }

    public static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
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
            .AddSingleton<Func<TemplateAppDbContext>>(
                provider =>
                    () =>
                        new TemplateAppDbContext(
                            provider.GetRequiredService<DbContextOptions<TemplateAppDbContext>>(),
                            provider.GetRequiredService<IUserAccessor>()
                        )
            )
            .RegisterRetryHelper();

        SetupHangfire.AddHangfire(services, connectionString, configuration);
        AddSeeders(services, configuration);
        PostgresSerialization.AdjustDateOnlySerialization();
    }

    public static void AddSeeders(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DefaultUserOptions>(configuration.GetSection("DefaultUser"));
    }
}
