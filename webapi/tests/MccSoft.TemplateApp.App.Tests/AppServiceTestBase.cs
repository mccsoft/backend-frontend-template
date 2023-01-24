using System;
using System.Linq;
using System.Threading.Tasks;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.LowLevelPrimitives;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using MccSoft.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace MccSoft.TemplateApp.App.Tests;

/// <summary>
/// The base class for application service test classes.
/// </summary>
public class AppServiceTestBase : TestBase<TemplateAppDbContext>, IDisposable
{
    protected User _defaultUser;

    protected AppServiceTestBase(
        ITestOutputHelper outputHelper,
        DatabaseType? testDatabaseType = DatabaseType.Postgres
    ) : base(outputHelper, testDatabaseType)
    {
        Audit.Core.Configuration.AuditDisabled = true;

        // initialize some variables to be available in all tests
        if (testDatabaseType != null)
        {
            TaskUtils.RunSynchronously(async () =>
            {
                await WithDbContext(async db =>
                {
                    _defaultUser = await db.Users.FirstAsync(x => x.Email == "default@test.test");
                });
                _userAccessorMock.Setup(x => x.GetUserId()).Returns(_defaultUser.Id);
                _userAccessorMock.Setup(x => x.GetTenantId()).Returns(_defaultUser.TenantId);
            });
        }
    }

    /// <summary>
    /// Insert some data that you want to be available in every test.
    ///
    /// Note, that this method is executed only once, when template database is initially created.
    /// !!IT IS NOT CALLED IN EACH TEST!!!
    /// (though, created data is available in each test by backing up
    /// and restoring a DB from template for each test)
    /// </summary>
    protected override DatabaseSeedingOptions<TemplateAppDbContext> SeedDatabase() =>
        new DatabaseSeedingOptions<TemplateAppDbContext>(
            nameof(TemplateAppDbContext) + "AppServiceTest",
            async (db) =>
            {
                var tenant = new Tenant();
                db.Tenants.Add(tenant);
                await db.SaveChangesAsync();

                var user = new User("default@test.test");
                db.Users.Add(user);
                user.SetTenantIdUnsafe(tenant.Id);
                await db.SaveChangesAsync();
            },
            CreateDbContext
        );

    protected override void RegisterServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        SetupServices.AddServices(services, configuration, environment);

        // Here you could override type registration (e.g. mock http clients that call other microservices).
        // Most probably you'd need to remove existing registration before registering new one.
        // You could remove the registration by calling:
        // services.RemoveRegistration<TService>();
    }

    /// <summary>
    /// Creates a new instance of <see cref="TemplateAppDbContext"/>.
    /// </summary>
    /// <returns>A new DbContext instance.</returns>
    protected override TemplateAppDbContext CreateDbContext(
        DbContextOptions<TemplateAppDbContext> options
    )
    {
        return new TemplateAppDbContext(options, _userAccessorMock.Object);
    }
}
