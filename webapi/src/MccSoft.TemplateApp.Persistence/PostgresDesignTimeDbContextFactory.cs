using MccSoft.LowLevelPrimitives;
using MccSoft.TemplateApp.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.TemplateApp.Persistence;

/// <summary>
/// This class is to allow running powershell EF commands from the project folder without
/// specifying Startup class (without triggering the whole startup during EF operations
/// like add/remove migrations).
/// </summary>
public class PostgresDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TemplateAppDbContext>
{
    public TemplateAppDbContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDefaultIdentity<User>();
        services.AddSingleton<IUserAccessor>(sp => null);
        services.AddDbContext<TemplateAppDbContext>(
            (provider, opt) =>
            {
                opt.UseNpgsql(
                    "Server=localhost;Database=template_app;Port=5432;Username=postgres;Password=postgres;Pooling=true;Keepalive=5;Command Timeout=60;",
                    builder => TemplateAppDbContext.MapEnums(builder)
                );
                opt.UseOpenIddict();
            }
        );
        var dbContext = services.BuildServiceProvider().GetRequiredService<TemplateAppDbContext>();
        return dbContext;
    }
}
