using MccSoft.Logging;
using MccSoft.Mailing;
using MccSoft.PersistenceHelpers.DomainEvents;
using MccSoft.TemplateApp.App;
using MccSoft.TemplateApp.App.DomainEventHandlers;
using MccSoft.TemplateApp.App.Setup;
using MccSoft.WebApi.Sentry;
using MccSoft.WebApi.Serialization.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", true).AddEnvironmentVariables();

builder.Host.UseSerilog(
    (hostingContext, loggerConfiguration) =>
    {
        loggerConfiguration.ConfigureSerilog(
            hostingContext.HostingEnvironment,
            hostingContext.Configuration
        );
    }
);

SetupDatabase.AddDatabase(builder);
SetupAudit.ConfigureAudit(builder.Services, builder.Configuration);
builder.Services.AddDomainEventsWithMediatR(typeof(Program), typeof(LogDomainEventHandler));

SetupAuth.ConfigureAuth(builder);
SetupLocalization.AddLocalization(builder);

builder.Services.AddUtcEverywhere();

builder.Services.AddMailing(builder.Configuration.GetSection("Email"));

SetupAspNet.AddAspNet(builder);

SetupSwagger.AddSwagger(builder);

// Set up your application-specific services here
SetupServices.AddServices(builder);

// ---------------------------------
//
var app = builder.Build();

// ---------------------------------

app.UseSerilog(app.Environment);
app.Logger.LogSentryTestError("TemplateApp");

await SetupDatabase.RunMigration(app);
SetupHangfire.UseHangfire(app);

app.UseHttpsRedirection();

SetupAspNet.UseFrontlineServices(app);

SetupLocalization.UseLocalization(app);
app.UseStaticFiles(SetupStaticFiles.CacheAll);

SetupAuth.UseAuth(app);
SetupSwagger.UseSwagger(app);

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
});

app.Logger.LogInformation("Service started");
SetupAudit.HttpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T> in tests
}
