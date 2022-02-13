using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MccSoft.AdminService.ComponentTests.Infrastructure;
using MccSoft.TemplateApp.ComponentTests.Infrastructure;
using MccSoft.TemplateApp.Http;
using MccSoft.TemplateApp.Persistence;
using MccSoft.Testing.SqliteUtils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace MccSoft.TemplateApp.ComponentTests
{
    public class TestBase : Testing.TestBase, IDisposable
    {
        private const string _host = "localhost";
        private const int _port = 5500;

        /// <summary>
        /// A DI-scope whose lifetime corresponds to the lifetime of the test.
        /// (It is created and disposed together with the test class).
        /// </summary>
        protected readonly IServiceScope TestScope;

        protected readonly HttpClient Client;
        protected readonly AuthenticationClient AuthenticationClient;

        protected readonly TestServer TestServer;
        protected Mock<IBackgroundJobClient> _backgroundJobClient;

        protected TestBase()
        {
            SeedAndBackupDatabaseOnFirstRun();

            _databaseFileName = $"test-{Guid.NewGuid()}.sqlite";
            var applicationFactory = new CustomWebApplicationFactory(
                ConfigureServices,
                _databaseFileName
            );
            Client = applicationFactory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri($"http://{_host}:{_port}")
                }
            );
            Client.Timeout = TimeSpan.FromSeconds(300);
            TestServer = applicationFactory.Server;
            TestScope = TestServer.Host.Services.CreateScope();
            Configuration = ResolveFromTestScope<IConfiguration>();
            AuthenticationClient = new AuthenticationClient(Client);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var db = ResolveFromTestScope<TemplateAppDbContext>();
            // var adminUser = db.Users.First(x => x.UserName == "admin");
            // _defaultUserId = adminUser.Id;
            // Identity.SetUserIdClaim(_defaultUserId);
        }

        #region Seed And Backup Database on first run

        private static object _lockFirstInitialize = new object();
        private static Task _firstInitializationTask;
        private readonly string _databaseFileName;
        private const string _backupSqliteFileName = "_master.sqlite";

        private void SeedAndBackupDatabaseOnFirstRun()
        {
            if (_firstInitializationTask == null)
            {
                lock (_lockFirstInitialize)
                {
                    if (_firstInitializationTask == null)
                    {
                        var firstInitializationTaskCompletionSource = new TaskCompletionSource();
                        _firstInitializationTask = firstInitializationTaskCompletionSource.Task;

                        SeedAndBackupDatabase();

                        firstInitializationTaskCompletionSource.SetResult();
                    }
                }
            }

            _firstInitializationTask.Wait();
        }

        private void SeedAndBackupDatabase()
        {
            var filename = $"{Guid.NewGuid()}.sqlite";
            using (
                var applicationFactory = new CustomWebApplicationFactory(
                    ConfigureServices,
                    filename
                )
            )
            {
                using (TestServer server = applicationFactory.Server)
                {
                    server.Services
                        .GetRequiredService<SqliteConnectionHolder>()
                        .BackupDatabase(_backupSqliteFileName);
                }
            }

            File.Delete(filename);
        }

        #endregion

        protected IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the service of type <typeparamref name="T"/> from the scope associated
        /// with the entire test.
        /// </summary>
        /// <typeparam name="T">The type of the service resolved from the DI container.</typeparam>
        /// <returns>An instance of the scoped service.</returns>
        protected T ResolveFromTestScope<T>()
        {
            return TestScope.ServiceProvider.GetRequiredService<T>();
        }

        protected void WithDbContext(Action<TemplateAppDbContext> action)
        {
            using var scope = TestServer.Host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TemplateAppDbContext>();
            action(db);
        }

        protected T WithDbContext<T>(Func<TemplateAppDbContext, T> action)
        {
            using var scope = TestServer.Host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TemplateAppDbContext>();
            return action(db);
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
            // Do not dispose of TestServer, it is disposed together with the applicationFactory.
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            _backgroundJobClient = new Mock<IBackgroundJobClient>();
            services.AddSingleton(_backgroundJobClient.Object);
        }
    }
}
