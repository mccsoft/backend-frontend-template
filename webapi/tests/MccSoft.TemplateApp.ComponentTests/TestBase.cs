using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hangfire;
using MccSoft.IntegreSql.EF;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.TemplateApp.App.Setup;
using MccSoft.TemplateApp.ComponentTests.Infrastructure;
using MccSoft.TemplateApp.Http;
using MccSoft.TemplateApp.Persistence;
using MccSoft.Testing.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NeinLinq;
using Xunit.Abstractions;

namespace MccSoft.TemplateApp.ComponentTests
{
    public class TestBase : Testing.ComponentTestBase<TemplateAppDbContext, Program>, IDisposable
    {
        protected AuthenticationClient AuthenticationClient;

        protected Mock<IBackgroundJobClient> _backgroundJobClient;

        private readonly ITestOutputHelper _outputHelper;
        private readonly bool _usePostgres = false;
        private readonly IDatabaseInitializer _databaseInitializer;
        private readonly string _connectionString;

        protected TestBase(ITestOutputHelper outputHelper, bool usePostgres = true)
        {
            _outputHelper = outputHelper;
            _usePostgres = usePostgres;
            _databaseInitializer = _usePostgres
                ? new NpgsqlDatabaseInitializer(
                    connectionStringOverride: new() { Host = "localhost", Port = 5434, }
                )
                : new SqliteDatabaseInitializer();

            _connectionString = _databaseInitializer.CreateDatabaseGetConnectionStringSync(
                new DatabaseSeedingOptions<TemplateAppDbContext>(
                    Name: nameof(TestBase),
                    SeedingFunction: async (dbContext) =>
                    {
                        CreateWebApplicationFactory(
                            dbContext.Database.GetConnectionString()!,
                            enableMigrations: true
                        );
                        await Task.CompletedTask;
                    },
                    DisableEnsureCreated: true
                )
            );

            CreateWebApplicationFactory(_connectionString);
        }

        /// <summary>
        /// Runs after WebApplication initialization, but before any test logic.
        /// It's a good place to initialize TestClient or some other global variables that will be used in tests
        /// </summary>
        protected override void InitializeGlobalVariables(
            WebApplicationFactory<Program> application
        )
        {
            base.InitializeGlobalVariables(application);
            AuthenticationClient = new AuthenticationClient(Client);
        }

        private ComponentTestFixture CreateWebApplicationFactory(
            string connectionString,
            bool enableMigrations = false
        )
        {
            var envVariables = enableMigrations
                ? null
                : new Dictionary<string, string>
                {
                    { SetupDatabase.DisableMigrationOptionName, true.ToString() },
                    { SetupHangfire.DisableHangfireOptionName, true.ToString() },
                    { SetupAuth.DisableSignOutLockedUserMiddleware, true.ToString() },
                };

            ComponentTestFixture application = new ComponentTestFixture()
            {
                OutputHelper = _outputHelper,
                OverrideServices = (services) =>
                {
                    ConfigureServices(services);
                    services.RemoveDbContextRegistration<TemplateAppDbContext>();

                    services.AddDbContext<TemplateAppDbContext>(
                        options =>
                        {
                            _databaseInitializer.UseProvider(options, connectionString);
                            options.WithLambdaInjection();
                            options.UseOpenIddict();
                        },
                        contextLifetime: ServiceLifetime.Scoped,
                        optionsLifetime: ServiceLifetime.Singleton
                    );
                },
                EnvVariables = envVariables
            };

            InitializeGlobalVariables(application);
            return application;
        }

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
            _databaseInitializer.RemoveDatabase(_connectionString);
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
