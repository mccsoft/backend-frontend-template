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

    private readonly List<Action<IWebHostBuilder>> _configurationActions = new();

    public ComponentTestFixture()
    {
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    public ComponentTestFixture Configure(Action<IWebHostBuilder> configureBuilder)
    {
        _configurationActions.Add(configureBuilder);
        return this;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders().AddXUnit(this));
        builder.UseEnvironment("Test");

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
        });
        _configurationActions.ForEach(x => x.Invoke(builder));
    }
}
