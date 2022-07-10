using MccSoft.TemplateApp.App.Services.Authentication;
using Microsoft.Extensions.Options;

namespace MccSoft.TemplateApp.App.Setup;

/// <summary>
/// This class will not be touched during pulling changes from Template.
/// Consider putting your project-specific code here.
/// </summary>
public static partial class SetupDatabase
{
    private static async partial Task RunMigrationsProjectSpecific(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

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
    }

    static partial void AddProjectSpecifics(WebApplicationBuilder builder) { }

    static partial void AddSeeders(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DefaultUserOptions>(configuration.GetSection("DefaultUser"));
    }
}
