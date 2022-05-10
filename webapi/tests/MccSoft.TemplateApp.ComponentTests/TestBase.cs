using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MccSoft.AdminService.ComponentTests.Infrastructure;
using MccSoft.IntegreSqlClient;
using MccSoft.IntegreSqlClient.DatabaseInitialization;
using MccSoft.TemplateApp.ComponentTests.Infrastructure;
using MccSoft.TemplateApp.Http;
using MccSoft.TemplateApp.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NeinLinq;

namespace MccSoft.TemplateApp.ComponentTests
{
    public class TestBase
        : Testing.ComponentTestBase<TemplateAppDbContext, TestStartup>,
          IDisposable
    {
        protected AuthenticationClient AuthenticationClient;

        protected Mock<IBackgroundJobClient> _backgroundJobClient;

        private readonly bool _usePostgres = false;
        private readonly IDatabaseInitializer _databaseInitializer;

        protected TestBase(bool usePostgres = true)
        {
            _usePostgres = usePostgres;
            _databaseInitializer = _usePostgres
                ? new NpgsqlDatabaseInitializer()
                : new SqliteDatabaseInitializer();
            NpgsqlDatabaseInitializer.ConnectionStringOverride = new ConnectionStringOverride()
            {
                Host = "localhost",
                Port = 5434,
            };
            CreateApplicationWithAdvancedSeeding(
                    new AdvancedDatabaseSeedingOptions("WithSeeding", () => Task.CompletedTask)
                )
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Runs after WebApplication initialization, but before any test logic.
        /// It's a good place to initialize TestClient or some other global variables that will be used in tests
        /// </summary>
        protected override void InitializeGlobalVariables(
            WebApplicationFactory<TestStartup> application
        )
        {
            base.InitializeGlobalVariables(application);
            AuthenticationClient = new AuthenticationClient(Client);
        }

        #region CreateApplication

        /// <summary>
        /// Creates Web application (i.e. TestServer, Client, etc.) using custom database seeding function.
        /// Works fast because database seeding is only done once and database is cached after that.
        /// </summary>
        /// <param name="databaseSeedingOptions"></param>
        private void CreateApplicationWithBasicSeeding(
            BasicDatabaseSeedingOptions<TemplateAppDbContext>? databaseSeedingOptions = null
        )
        {
            CreateApplication(
                options =>
                {
                    _databaseInitializer.UseProvider(options, databaseSeedingOptions);
                },
                enableSeedingInStartup: true
            );
        }

        protected record AdvancedDatabaseSeedingOptions(string Name, Func<Task> SeedingFunction);

        /// <summary>
        /// This is similar to <see cref="CreateApplicationWithBasicSeeding"/>, but we assume that seeding function here
        /// is quite complex and calls backend API during seeding (thus, it requires
        /// <see cref="TestServer"/> and <see cref="Client"/> to be initialized).
        /// Use in advanced scenarios, for simple ones prefer passing <see cref="BasicDatabaseSeedingOptions{TDbContext}"/> to TestBase constructor!
        /// </summary>
        /// <param name="databaseSeedingOptions"></param>
        protected async Task CreateApplicationWithAdvancedSeeding(
            AdvancedDatabaseSeedingOptions databaseSeedingOptions
        )
        {
            var connectionString = await _databaseInitializer.GetConnectionString(
                databaseSeedingOptions.Name
                    + ContextHelper.GetLastMigrationName<TemplateAppDbContext>(),
                async (connectionString) =>
                {
                    CreateApplication(
                        (options) =>
                        {
                            _databaseInitializer.UseProvider(options, connectionString);
                        },
                        enableSeedingInStartup: true
                    );
                    await databaseSeedingOptions.SeedingFunction();
                }
            );

            CreateApplication(
                (options) =>
                {
                    _databaseInitializer.UseProvider(options, connectionString);
                }
            );
        }

        private CustomWebApplicationFactory CreateApplication(
            Action<DbContextOptionsBuilder> configureDatabaseOptions,
            bool enableSeedingInStartup = false
        )
        {
            CustomWebApplicationFactory application = new CustomWebApplicationFactory(
                (services) =>
                {
                    ConfigureServices(services);
                    services.AddDbContext<TemplateAppDbContext>(
                        opt =>
                        {
                            configureDatabaseOptions(opt);

                            opt.WithLambdaInjection();
                            opt.UseOpenIddict();
                        },
                        contextLifetime: ServiceLifetime.Scoped,
                        optionsLifetime: ServiceLifetime.Singleton
                    );
                },
                enableSeedingInStartup
                  ? null
                  : new Dictionary<string, string> { { "DisableSeed", true.ToString() } }
            );

            InitializeGlobalVariables(application);
            return application;
        }

        #endregion


        /// <summary>
        /// If you'd like to stub authorized user with certain Id you could do it like:
        /// Identity.AddClaims("id", "1231-123-123")
        /// </summary>
        public ClaimsIdentity Identity
        {
            get => AuthenticationOptions.Identity;
            set => AuthenticationOptions.Identity = value;
        }

        private TestAuthenticationOptions AuthenticationOptions =>
            ResolveFromTestScope<IOptionsMonitor<TestAuthenticationOptions>>()
                .Get(TestAuthenticationOptions.Scheme);

        public void Dispose()
        {
            TestScope.Dispose();
            _databaseInitializer.Dispose();
            // Do not dispose of TestServer, it is disposed together with the applicationFactory.
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            _backgroundJobClient = new Mock<IBackgroundJobClient>();
            services.AddSingleton(_backgroundJobClient.Object);
        }
    }
}
