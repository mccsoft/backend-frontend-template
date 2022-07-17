using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Hangfire;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.NpgSql;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using MccSoft.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

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
            : base(
                testDatabaseType,
                new DatabaseSeedingOptions<TemplateAppDbContext>(
                    Name: "DefaultUser",
                    SeedingFunction: async db =>
                    {
                        db.Users.Add(new User("default@test.test"));
                        await db.SaveChangesAsync();
                    }
                )
            )
        {
            if (testDatabaseType != null)
            {
                WithDbContextSync(db =>
                {
                    _defaultUser = db.Users.First(x => x.Email == "default@test.test");
                });
                _userAccessorMock.Setup(x => x.GetUserId()).Returns(_defaultUser.Id);
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            PostgresSerialization.AdjustDateOnlySerialization();
            Audit.Core.Configuration.AuditDisabled = true;
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
