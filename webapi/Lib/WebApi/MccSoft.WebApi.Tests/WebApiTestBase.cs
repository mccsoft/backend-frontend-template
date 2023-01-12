using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

public class WebApiTestBase : TestBase<MyDbContext>
{
    public WebApiTestBase(ITestOutputHelper outputHelper, DatabaseType? databaseType)
        : base(outputHelper, databaseType) { }

    protected override DatabaseSeedingOptions<MyDbContext> SeedDatabase()
    {
        return new DatabaseSeedingOptions<MyDbContext>("Custom");
    }

    protected override MyDbContext CreateDbContext(DbContextOptions<MyDbContext> options)
    {
        return new MyDbContext(options);
    }

    protected override void RegisterServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        services.AddTransient<HangfireStubTestService>();
    }
}
