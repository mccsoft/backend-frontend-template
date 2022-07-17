using System.Linq;
using System.Threading.Tasks;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.NpgSql;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using MccSoft.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.TemplateApp.App.Tests
{
    /// <summary>
    /// The base class for application service test classes.
    /// </summary>
    /// <typeparam name="TService">The type of the service under test.</typeparam>
    public class AppServiceTestBase<TService> : AppServiceTestBase<TService, TemplateAppDbContext>
        where TService : class
    {
        protected User _defaultUser;

        public AppServiceTestBase(DatabaseType? testDatabaseType = DatabaseType.Postgres)
            : base(testDatabaseType)
        {
            if (testDatabaseType != null)
            {
                InitializeDatabase(
                    new DatabaseSeedingOptions<TemplateAppDbContext>(
                        Name: "DefaultUser",
                        SeedingFunction: SeedDatabase
                    )
                );
            }

            PostgresSerialization.AdjustDateOnlySerialization();
            Audit.Core.Configuration.AuditDisabled = true;
        }

        protected virtual async Task SeedDatabase(TemplateAppDbContext db)
        {
            db.Users.Add(new User("default@test.test"));
            await db.SaveChangesAsync();
        }

        protected override void InitializeGlobalVariables()
        {
            WithDbContextSync(db =>
            {
                _defaultUser = db.Users.First(x => x.Email == "default@test.test");
            });
            _userAccessorMock.Setup(x => x.GetUserId()).Returns(_defaultUser.Id);
        }

        protected override ServiceCollection CreateServiceCollection()
        {
            var serviceCollection = base.CreateServiceCollection();

            // Here you could register more project-specific types in a service collection

            return serviceCollection;
        }

        /// <summary>
        /// Creates a new instance of <see cref="TemplateAppDbContext"/>.
        /// Should be used in alternative implementations of <see cref="InitializeService" />.
        /// </summary>
        /// <returns>A new DbContext instance.</returns>
        protected override TemplateAppDbContext CreateDbContext()
        {
            return new TemplateAppDbContext(_builder.Options, _userAccessorMock.Object);
        }
    }
}
