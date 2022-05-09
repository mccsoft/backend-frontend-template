using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using MccSoft.PersistenceHelpers.DomainEvents;
using MccSoft.TemplateApp.App;
using MccSoft.TemplateApp.Persistence;
using MccSoft.Testing.SqliteUtils;
using Microsoft.Extensions.Configuration;
using Moq;
using NeinLinq;

namespace MccSoft.TemplateApp.ComponentTests.Infrastructure
{
    public class CustomWebApplicationFactory : WebApplicationFactory<TestStartup>
    {
        private readonly Action<IServiceCollection> _overrideServices;
        private readonly Dictionary<string, string> _envVariables;

        public CustomWebApplicationFactory(
            Action<IServiceCollection> overrideServices,
            Dictionary<string, string> envVariables = null
        )
        {
            _overrideServices = overrideServices;
            _envVariables = envVariables;
        }

        protected override IWebHostBuilder CreateWebHostBuilder() =>
            Program
                .CreateWebHostBuilder<TestStartup>(args: null)
                .ConfigureAppConfiguration(
                    (hostingContext, config) =>
                    {
                        if (_envVariables != null)
                            config.AddInMemoryCollection(_envVariables);
                    }
                )
                .UseEnvironment("Test");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot(
                solutionRelativePath: "src/MccSoft.TemplateApp.App",
                applicationBasePath: "../../../../../"
            );
            builder.ConfigureTestServices(_overrideServices);
        }
    }
}
