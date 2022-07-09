using MccSoft.TemplateApp.App.Services.Authentication;

namespace MccSoft.TemplateApp.App.Setup;

/// <summary>
/// This class will not be touched during pulling changes from Template.
/// Consider putting your project-specific code here.
/// </summary>
public static partial class SetupDatabase
{
    static partial void RunMigrationsProjectSpecific(WebApplication app) { }

    static partial void AddProjectSpecifics(WebApplicationBuilder builder) { }

    static partial void AddSeeders(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DefaultUserOptions>(configuration.GetSection("DefaultUser"));
    }
}
