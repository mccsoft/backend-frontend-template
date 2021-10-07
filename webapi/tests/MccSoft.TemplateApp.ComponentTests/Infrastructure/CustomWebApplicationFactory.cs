using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using MccSoft.TemplateApp.App;
using MccSoft.TemplateApp.Persistence;
using MccSoft.Testing.SqliteUtils;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Moq;

namespace MccSoft.TemplateApp.ComponentTests.Infrastructure
{
    public class CustomWebApplicationFactory : WebApplicationFactory<TestStartup>
    {
        private readonly Action<IServiceCollection> _overrideServices;
        private readonly string _databaseFileName;

        public CustomWebApplicationFactory(
            Action<IServiceCollection> overrideServices,
            string databaseFileName
        ) {
            _overrideServices = overrideServices;
            _databaseFileName = databaseFileName;
        }

        protected override IWebHostBuilder CreateWebHostBuilder() =>
            Program.CreateWebHostBuilder<TestStartup>(args: null).UseEnvironment("Test");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot(
                solutionRelativePath: "src/MccSoft.TemplateApp.App",
                applicationBasePath: "../../../../../"
            );

            builder.ConfigureTestServices(
                services =>
                {
                    var clientRequestParametersProvider =
                        new Mock<IClientRequestParametersProvider>();
                    services.AddSingleton<IClientRequestParametersProvider>(
                        clientRequestParametersProvider.Object
                    );

                    _overrideServices?.Invoke(services);

                    services.AddSqliteInMemory<TemplateAppDbContext>(_databaseFileName);
                }
            );
        }
    }
}
