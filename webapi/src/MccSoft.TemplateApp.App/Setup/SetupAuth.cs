using MccSoft.TemplateApp.App.Middleware;
using MccSoft.TemplateApp.App.Utils.Localization;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using MccSoft.WebApi.SignedUrl;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Shaddix.OpenIddict.ExternalAuthentication.Cookies;
using Shaddix.OpenIddict.ExternalAuthentication.Infrastructure;

namespace MccSoft.TemplateApp.App.Setup;

public static partial class SetupAuth
{
    public const string DisableSignOutLockedUserMiddleware = nameof(
        DisableSignOutLockedUserMiddleware
    );

    public static void ConfigureAuth(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // uncommenting next line breaks AzureAD authentication (i.e. any external OpenId)
        // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services
            .AddDataProtection(x =>
            {
                x.ApplicationDiscriminator = nameof(TemplateApp);
            })
            .PersistKeysToFileSystem(
                new DirectoryInfo(
                    Path.Combine(
                        configuration.GetValue<string>("DefaultFileStorage"),
                        "data-protection-keys"
                    )
                )
            );

        services
            .AddDefaultIdentity<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.AllowedForNewUsers = false;

                // configure password security rules
                configuration.GetSection("OpenId:Password").Bind(options.Password);
            })
            .AddErrorDescriber<LocalizableIdentityErrorDescriber>()
            .AddRoles<IdentityRole>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddEntityFrameworkStores<TemplateAppDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

        // Configure Identity to use the same JWT claims as OpenIddict instead
        // of the legacy WS-Federation claims it uses by default (ClaimTypes),
        // which saves you from doing the mapping in your authorization controller.
        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
            options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
        });
        string siteUrl = configuration.GetSection("General").GetValue<string>("SiteUrl");

        services
            .AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore().UseDbContext<TemplateAppDbContext>();
            })
            .AddDefaultAuthorizationController(options =>
            {
                options
                    .SetPublicUrl(siteUrl)
                    .SetConfiguration(configuration.GetSection("OpenId"))
                    .DisableSetIssuerToPublicUrl();

                ConfigureDefaultAuthorizationController(options);
            })
            .AddSupportForHttpOnlyCookieClients()
            .AddServer(options =>
            {
                options
                    .DisableAccessTokenEncryption()
                    .AddSigningCertificateFromConfiguration(
                        configuration.GetSection("OpenId:SigningCertificate")
                    )
                    .AddEncryptionCertificateFromConfiguration(
                        configuration.GetSection("OpenId:EncryptionCertificate")
                    );
                if (configuration.GetValue<bool>("TestApiEnabled"))
                    options.AllowPasswordFlow();

                ConfigureOpenIdDict(builder, options);
            })
            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme =
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        // Make OpenIddict a default Authorization policy
        // (so that you could use [Authorize] without specifying scheme
        services.AddAuthorization();

        var signUrlSecret = configuration.GetSection("SignUrl").GetValue<string>("Secret");
        if (!string.IsNullOrEmpty(signUrlSecret))
            services.AddSignUrl(signUrlSecret);

        // If you'd like to modify this class, consider adding your custom code in the SetupAuth.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        AddProjectSpecifics(builder);
    }

    static partial void ConfigureOpenIdDict(
        WebApplicationBuilder webApplicationBuilder,
        OpenIddictServerBuilder options
    );

    static partial void AddProjectSpecifics(WebApplicationBuilder builder);

    static partial void UseProjectSpecificsBefore(WebApplication app);

    static partial void UseProjectSpecifics(WebApplication app);

    static partial void ConfigureDefaultAuthorizationController(OpenIddictSettings options);

    public static void UseAuth(WebApplication app)
    {
        // If you'd like to modify this class, consider adding your custom code in the SetupAuth.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        UseProjectSpecificsBefore(app);

        app.UseAuthentication();

        if (!app.Configuration.GetValue<bool>(DisableSignOutLockedUserMiddleware))
        {
            app.UseSignOutLockedUser();
        }

        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            IdentityModelEventSource.ShowPII = true;
        }

        // If you'd like to modify this class, consider adding your custom code in the SetupAuth.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        UseProjectSpecifics(app);
    }
}
