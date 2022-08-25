using System.Net;
using MccSoft.LowLevelPrimitives.Serialization.DateOnlyConverters;
using MccSoft.TemplateApp.App.Middleware;
using MccSoft.WebApi;
using MccSoft.WebApi.Sentry;
using MccSoft.WebApi.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Newtonsoft.Json;

namespace MccSoft.TemplateApp.App.Setup;

public static partial class SetupAspNet
{
    public static void AddAspNet(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.WebHost.UseAppSentry();
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<GzipCompressionProvider>();
        });

        JsonConvert.DefaultSettings = () =>
            JsonSerializerSetup.SetupJson(new JsonSerializerSettings());

        services
            .AddControllers(
                (options) =>
                {
                    AddGlobalFilters(options);
                    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                }
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

        services.AddMiniProfiler();

        // If you'd like to modify this class, consider adding your custom code in the SetupAspNet.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        AddProjectSpecifics(builder);
    }

    static partial void AddProjectSpecifics(WebApplicationBuilder builder);

    static partial void UseProjectSpecifics(WebApplication app);

    static partial void UseProjectSpecificEndpoints(WebApplication app);

    static partial void AddEndpoints(IEndpointRouteBuilder endpoints);

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

    public static void UseFrontlineServices(WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("DisableCompression"))
            app.UseResponseCompression();

        UseForwardedHeaders(app);
        app.UseRouting();
        app.UseSentryTracing();
        app.UseCors(DefaultCorsPolicyName);

        app.UseErrorHandling();
        app.UseRethrowErrorsFromPersistence();

        if (app.Configuration.GetValue<bool>("MiniProfilerEnabled"))
        {
            app.UseMiniProfiler();
        }

        // If you'd like to modify this class, consider adding your custom code in the SetupAspNet.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        UseProjectSpecifics(app);
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
        // forwardedHeadersOptions.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:10.0.0.0"), 104));
        // forwardedHeadersOptions.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:192.168.0.0"), 112));
        // forwardedHeadersOptions.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:172.16.0.0"), 108));
        forwardedHeadersOptions.KnownNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();

        app.UseForwardedHeaders(forwardedHeadersOptions);
    }

    public static void UseEndpoints(WebApplication app)
    {
        app.UseStaticFiles(SetupStaticFiles.CacheAll);

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints
                .MapRazorPages()
                .RequireAuthorization(
                    new AuthorizeAttribute() { AuthenticationSchemes = "Identity.Application" }
                );
            endpoints.MapHealthChecks("/health");
            endpoints.MapFallbackToFile("index.html", SetupStaticFiles.DoNotCache);

            AddEndpoints(endpoints);
        });
        app.Use404ForMissingStaticFiles();

        UseProjectSpecificEndpoints(app);
    }
}
