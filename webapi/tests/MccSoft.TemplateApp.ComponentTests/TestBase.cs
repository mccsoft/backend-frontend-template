using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using MccSoft.IntegreSql.EF;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.TemplateApp.App.Setup;
using MccSoft.TemplateApp.ComponentTests.Infrastructure;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Http;
using MccSoft.TemplateApp.Persistence;
using MccSoft.Testing.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NeinLinq;
using OpenIddict.Abstractions;
using Xunit.Abstractions;

namespace MccSoft.TemplateApp.ComponentTests
{
    public class TestBase : Testing.ComponentTestBase<TemplateAppDbContext, Program>, IDisposable
    {
        protected AuthenticationClient AuthenticationClient;

        protected Mock<IBackgroundJobClient> _backgroundJobClient;

        protected readonly ITestOutputHelper _outputHelper;
        private readonly bool _usePostgres = false;
        private readonly IDatabaseInitializer _databaseInitializer;
        private readonly string _connectionString;
        protected User _defaultUser;

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

            var context = ResolveFromTestScope<TemplateAppDbContext>();
            _defaultUser = context.Users.First(u => u.UserName == "admin");

            Identity.RemoveClaims("sub");
            Identity.AddClaim("sub", _defaultUser.Id);
        }

        private ComponentTestFixture CreateWebApplicationFactory(
            string connectionString,
            bool enableMigrations = false
        )
        {
            ComponentTestFixture application = new ComponentTestFixture()
            {
                OutputHelper = _outputHelper,
            }
                .ConfigureTestServices(services =>
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
                })
                .UseSetting("ConnectionStrings:DefaultConnection", connectionString);

            if (!enableMigrations)
            {
                application
                    .UseSetting(SetupDatabase.DisableMigrationOptionName, "true")
                    .UseSetting("Hangfire:Disable", "true")
                    .UseSetting(SetupAuth.DisableSignOutLockedUserMiddleware, "true");
            }

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
            services.AddTransient<BasicApiTests.HangfireStubTestService>();

            _backgroundJobClient = new Mock<IBackgroundJobClient>();
            services.AddSingleton(_backgroundJobClient.Object);

            _backgroundJobClient
                .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
                .Callback(
                    (Job job, IState state) =>
                    {
                        using var scope = TestScope.ServiceProvider.CreateScope();
                        object? result;
                        if (job.Method.IsStatic)
                        {
                            result = job.Method.Invoke(null, job.Args.ToArray());
                        }
                        else
                        {
                            var service = scope.ServiceProvider.GetRequiredService(job.Type);
                            result = job.Method.Invoke(service, job.Args.ToArray());
                        }
                        if (result is Task resultTask)
                        {
                            resultTask.GetAwaiter().GetResult();
                        }
                    }
                );
        }
    }
}
