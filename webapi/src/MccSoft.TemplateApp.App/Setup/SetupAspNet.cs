using System.Net;
using MccSoft.LowLevelPrimitives.Serialization.DateOnlyConverters;
using MccSoft.TemplateApp.App.Middleware;
using MccSoft.WebApi;
using MccSoft.WebApi.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Newtonsoft.Json;

namespace MccSoft.TemplateApp.App.Setup;

public static class SetupAspNet
{
    public static void AddAspNet(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        JsonConvert.DefaultSettings = () =>
            JsonSerializerSetup.SetupJson(new JsonSerializerSettings());

        services.AddControllers(AddGlobalFilters);

        services
            .AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter())
            )
            .AddNewtonsoftJson(
                setupAction => JsonSerializerSetup.SetupJson(setupAction.SerializerSettings)
            );

        services.AddRazorPages();
        if (builder.Environment.IsDevelopment())
        {
            services.AddRazorPages().AddRazorRuntimeCompilation();
        }

        services.AddHealthChecks();

        AddCors(builder);
    }

    private const string DefaultCorsPolicyName = "DefaultCorsPolicy";

    public static void AddCors(WebApplicationBuilder builder)
    {
        string siteUrl = builder.Configuration.GetSection("General").GetValue<string>("SiteUrl");
        string cors = builder.Configuration.GetSection("General").GetValue<string>("CORS");
        string[] additionalOrigins = string.IsNullOrEmpty(cors)
            ? new string[] { }
            : cors.Split(",");

        builder.Services.AddCors(
            x =>
                x.AddPolicy(
                    DefaultCorsPolicyName,
                    configurePolicy =>
                        configurePolicy
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .WithExposedHeaders("x-miniprofiler-ids")
                            .WithOrigins(additionalOrigins.Union(new[] { siteUrl }).ToArray())
                            .AllowCredentials()
                )
        );
    }

    private static void AddGlobalFilters(MvcOptions options)
    {
        // This has the effect of applying the [Authorize] attribute to all
        // controllers, so developers don't need to remember to add it manually.
        // Use the [AllowAnonymous] attribute on a specific controller to override.
        options.Filters.Add(new AuthorizeFilter());

        // This is needed for NSwag to produce correct client code
        options.Filters.Add(
            new ProducesResponseTypeAttribute(typeof(ValidationProblemDetails), 400)
        );
        options.Filters.Add(new ProducesResponseTypeAttribute(200));
    }

    public static void UseFrontlineServices(IApplicationBuilder app)
    {
        UseForwardedHeaders(app);
        app.UseRouting();
        app.UseCors(DefaultCorsPolicyName);

        app.UseErrorHandling();
        app.UseRethrowErrorsFromPersistence();
    }

    private static void UseForwardedHeaders(IApplicationBuilder app)
    {
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders =
                ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
                | ForwardedHeaders.XForwardedHost
        };
        // These three subnets encapsulate the applicable Azure subnets. At the moment, it's not possible to narrow it down further.
        // from https://docs.microsoft.com/en-us/azure/app-service/configure-language-dotnetcore?pivots=platform-linux
        forwardedHeadersOptions.KnownNetworks.Add(
            new IPNetwork(IPAddress.Parse("::ffff:10.0.0.0"), 104)
        );
        forwardedHeadersOptions.KnownNetworks.Add(
            new IPNetwork(IPAddress.Parse("::ffff:192.168.0.0"), 112)
        );
        forwardedHeadersOptions.KnownNetworks.Add(
            new IPNetwork(IPAddress.Parse("::ffff:172.16.0.0"), 108)
        );
        app.UseForwardedHeaders(forwardedHeadersOptions);
    }
}
