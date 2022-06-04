using System;
using System.Collections.Generic;
using MartinCostello.Logging.XUnit;
using MccSoft.TemplateApp.ComponentTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace MccSoft.TemplateApp.ComponentTests;

public class ComponentTestFixture : WebApplicationFactory<Program>, ITestOutputHelperAccessor
{
    public ITestOutputHelper OutputHelper { get; set; }
    public Action<IServiceCollection> OverrideServices { get; set; }
    public Dictionary<string, string> EnvVariables { get; set; }

    public ComponentTestFixture()
    {
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders().AddXUnit(this));
        builder
            .ConfigureAppConfiguration(
                (_, config) =>
                {
                    if (EnvVariables != null)
                        config.AddInMemoryCollection(EnvVariables);
                }
            )
            .UseEnvironment("Test");

        builder.UseSolutionRelativeContentRoot(
            solutionRelativePath: "src/MccSoft.TemplateApp.App",
            applicationBasePath: "../../../../../"
        );

        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationOptions.Scheme;
                    options.DefaultChallengeScheme = TestAuthenticationOptions.Scheme;
                })
                .AddTestAuthentication(options => { });

            OverrideServices?.Invoke(services);
        });
    }
}
