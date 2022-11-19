using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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
        var optionsBuilder = new DbContextOptionsBuilder<TemplateAppDbContext>();

        optionsBuilder
            .UseNpgsql(
                "Server=localhost;Database=template_app;Port=5432;Username=postgres;Password=postgres;Pooling=true;Keepalive=5;Command Timeout=60;"
            )
            .UseOpenIddict();

        return new TemplateAppDbContext(optionsBuilder.Options, null);
    }
}
