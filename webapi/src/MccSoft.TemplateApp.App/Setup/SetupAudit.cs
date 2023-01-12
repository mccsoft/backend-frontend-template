using Audit.Core;
using MccSoft.TemplateApp.App.Settings;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using OpenIddict.EntityFrameworkCore.Models;

namespace MccSoft.TemplateApp.App.Setup;

public static partial class SetupAudit
{
    public static IHttpContextAccessor HttpContextAccessor { get; set; }

    public static void ConfigureAudit(IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Audit");
        services.Configure<AuditSettings>(settings);
        var typedSettings = settings.Get<AuditSettings>();

        Configuration.AuditDisabled = !typedSettings.Enabled;

        //only one could be enabled at a time
        //SetupSavingToEfCore();
        SetupSavingToSerilog();

        Audit.EntityFramework.Configuration
            .Setup()
            .ForContext<TemplateAppDbContext>(
                config => config.ForEntity<User>(_ => _.Ignore(user => user.PasswordHash))
            )
            .UseOptOut()
            .Ignore<OpenIddictEntityFrameworkCoreToken>()
            .Ignore<OpenIddictEntityFrameworkCoreAuthorization>();
    }

    private static partial object CreateAuditMessageForSerilog(AuditEvent auditEvent, object arg2);

    private static partial TemplateAppDbContext CreateAuditDbContext(
        TemplateAppDbContext dbContext
    );

    public static void UseAudit(WebApplication app)
    {
        HttpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
    }
}
