using System.IdentityModel.Tokens.Jwt;
using MccSoft.TemplateApp.App.Middleware;
using MccSoft.TemplateApp.App.Utils.Localization;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using MccSoft.WebApi.SignedUrl;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Shaddix.OpenIddict.ExternalAuthentication.Infrastructure;

namespace MccSoft.TemplateApp.App.Setup;

public static class SetupAuth
{
    public const string DisableSignOutLockedUserMiddleware = nameof(
        DisableSignOutLockedUserMiddleware
    );

    public static void ConfigureAuth(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

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

        services
            .AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore().UseDbContext<TemplateAppDbContext>();
            })
            .AddDefaultAuthorizationController()
            .AddOpenIddictConfigurations(configuration)
            .AddServer(options =>
            {
                options.DisableAccessTokenEncryption();

                if (builder.Environment.IsDevelopment())
                {
                    options.UseAspNetCore().DisableTransportSecurityRequirement();
                }

                options.AddSigningCertificateFromConfiguration(configuration);
                options.AddEncryptionCertificateFromConfiguration(configuration);
            })
            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            })
            .AddGoogle(options =>
            {
                configuration.GetSection("ExternalAuthentication:Google").Bind(options);
            })
            .AddFacebook(options =>
            {
                configuration.GetSection("ExternalAuthentication").Bind("Facebook", options);
            })
            .AddMicrosoftAccount(options =>
            {
                configuration.GetSection("ExternalAuthentication:Microsoft").Bind(options);
            });

        // Make OpenIddict a default Authorization policy
        // (so that you could use [Authorize] without specifying scheme
        services.AddAuthorization();

        services.AddSignUrl(configuration.GetSection("SignUrl").GetValue<string>("Secret"));
    }

    public static void UseAuth(WebApplication app)
    {
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
    }
}
