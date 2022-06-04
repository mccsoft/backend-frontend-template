using MccSoft.Testing.Infrastructure;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.Testing
{
    /// <summary>
    /// A helper test class. Inherit from it to use <see cref="MotherFactory"/>.
    /// </summary>
    public class ComponentTestBase<TDbContext, TStartup> : TestBase<TDbContext>
        where TDbContext : DbContext
        where TStartup : class
    {
        protected TestServer TestServer;
        protected HttpClient Client;

        /// <summary>
        /// A DI-scope whose lifetime corresponds to the lifetime of the test.
        /// (It is created and disposed together with the test class).
        /// </summary>
        protected IServiceScope TestScope;
        protected IConfiguration Configuration { get; private set; }

        /// <summary>
        /// Runs after WebApplication initialization, but before any test logic.
        /// It's a good place to initialize TestClient or some other global variables that will be used in tests
        /// </summary>
        protected virtual void InitializeGlobalVariables(
            WebApplicationFactory<TStartup> application
        )
        {
            Client = application.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri($"https://localhost")
                }
            );
            Client.Timeout = TimeSpan.FromSeconds(300);
            TestServer = application.Server;

            TestScope = application.Services.CreateScope();
            Configuration = ResolveFromTestScope<IConfiguration>();

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

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

        protected override TDbContext CreateDbContext()
        {
            IServiceScope scope = TestServer.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
            return db;
        }
    }
}
