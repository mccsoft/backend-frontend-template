using MccSoft.LowLevelPrimitives;
using MccSoft.TemplateApp.App.Services.Authentication;
using MccSoft.TemplateApp.App.Services.Authentication.Seed;
using MccSoft.TemplateApp.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.TemplateApp.App.Setup;

/// <summary>
/// This class will not be touched during pulling changes from Template.
/// Consider putting your project-specific code here.
/// </summary>
public static partial class SetupDatabase
{
    public static async partial Task SeedDatabase(
        IServiceProvider serviceProvider,
        TemplateAppDbContext context
    )
    {
        DefaultUserSeeder seeder = serviceProvider.GetRequiredService<DefaultUserSeeder>();
        await seeder.SeedUser();
    }

    static partial void AddProjectSpecifics(WebApplicationBuilder builder) { }

    static partial void AddSeeders(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DefaultUserOptions>(configuration.GetSection("DefaultUser"));
    }

    public static partial TemplateAppDbContext CreateDbContext(IServiceProvider provider)
    {
        return provider.CreateScope().ServiceProvider.GetRequiredService<TemplateAppDbContext>();
    }
}
