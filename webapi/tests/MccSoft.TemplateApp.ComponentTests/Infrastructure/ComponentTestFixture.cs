using System;
using System.Collections.Generic;
using MartinCostello.Logging.XUnit;
using MccSoft.TemplateApp.ComponentTests.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace MccSoft.TemplateApp.ComponentTests;

public partial class ComponentTestFixture
    : WebApplicationFactory<Program>,
        ITestOutputHelperAccessor
{
    public ITestOutputHelper OutputHelper { get; set; }

    private readonly List<Action<IWebHostBuilder>> _configurationActions = new();

    public ComponentTestFixture()
    {
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = new Uri("https://localhost");
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

            // Disable all policies.
            services.RemoveAll<IPolicyEvaluator>();
            services.AddSingleton<IPolicyEvaluator, DisableAuthenticationPolicyEvaluator>();
        });

        

        // Consider adding your project-specific things in ConfigureWebHostProjectSpecific function
        // (defined in a ComponentTestFixture.partial.cs).
        // This way you will leave this file untouched which will make pulling changes from template easier
        ConfigureWebHostProjectSpecific(builder);

        _configurationActions.ForEach(x => x.Invoke(builder));
    }

    partial void ConfigureWebHostProjectSpecific(IWebHostBuilder builder);

    /// <summary>
    /// You can call this method from your TestBase to alter WebHostBuilder configuration
    /// </summary>
    public ComponentTestFixture Configure(Action<IWebHostBuilder> configureBuilder)
    {
        _configurationActions.Add(configureBuilder);
        return this;
    }

    /// <summary>
    /// Call this method to alter Services registration
    /// (this method is called AFTER configuring services in Program.cs
    /// </summary>
    public ComponentTestFixture ConfigureTestServices(
        Action<IServiceCollection> servicesConfiguration
    )
    {
        _configurationActions.Add(builder => builder.ConfigureTestServices(servicesConfiguration));
        return this;
    }

    /// <summary>
    /// Override appsettings.json configuration with provided key and value
    /// </summary>
    public ComponentTestFixture UseSetting(string key, string? value)
    {
        _configurationActions.Add(builder => builder.UseSetting(key, value));
        return this;
    }
}
