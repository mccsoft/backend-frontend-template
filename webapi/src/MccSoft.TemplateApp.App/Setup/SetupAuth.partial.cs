namespace MccSoft.TemplateApp.App.Setup;

/// <summary>
/// This class will not be touched during pulling changes from Template.
/// Consider putting your project-specific code here.
/// </summary>
public static partial class SetupAuth
{
    static partial void AddProjectSpecifics(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        services
            .AddAuthentication()
            .AddGoogle(options =>
            {
                configuration.GetSection("ExternalAuthentication:Google").Bind(options);
            })
            .AddFacebook(options =>
            {
                configuration.GetSection("ExternalAuthentication:Facebook").Bind(options);
            })
            .AddMicrosoftAccount(options =>
            {
                configuration.GetSection("ExternalAuthentication:Microsoft").Bind(options);
            })
            .AddOpenIdConnect(options =>
            {
                configuration.GetSection("ExternalAuthentication:AzureAd").Bind(options);
            });
    }

    static partial void UseProjectSpecifics(WebApplication app) { }
}
