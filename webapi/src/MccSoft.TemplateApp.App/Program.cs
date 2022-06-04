using MccSoft.DomainHelpers.DomainEvents.Events;
using MccSoft.Logging;
using MccSoft.Mailing;
using MccSoft.PersistenceHelpers.DomainEvents;
using MccSoft.TemplateApp.App;
using MccSoft.TemplateApp.App.Middleware;
using MccSoft.TemplateApp.App.Setup;
using MccSoft.WebApi;
using MccSoft.WebApi.Sentry;
using MccSoft.WebApi.Serialization.ModelBinding;
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

SetupDatabase.AddDatabase(builder.Services, builder.Configuration);
SetupAudit.ConfigureAudit(builder.Services, builder.Configuration);
builder.Services.AddDomainEventsWithMediatR(typeof(Program), typeof(LogDomainEventHandler));

SetupAuth.ConfigureAuth(builder);
SetupLocalization.AddLocalization(builder.Services);

builder.Services.AddUtcEverywhere();

builder.Services.AddMailing(builder.Configuration.GetSection("Email"));

SetupAspNet.AddAspNet(builder);

SetupSwagger.AddSwagger(builder.Services, builder.Configuration);

// builder.Services.AddSpaStaticFiles(configuration =>
// {
//     configuration.RootPath = "wwwroot";
// });

// Set up your application-specific services here
SetupServices.AddServices(builder.Services);

// ---------------------------------
//
var app = builder.Build();

// ---------------------------------

app.UseSerilog(app.Environment);
app.Logger.LogSentryTestError("TemplateApp");

await SetupDatabase.RunMigration(app);

SetupHangfire.UseHangfire(app);
SetupLocalization.UseLocalization(app);

SetupAspNet.UseFrontlineServices(app);

// app.UseDefaultFiles();
// app.UseStaticFiles();
// app.UseSpaStaticFiles();


SetupAuth.UseAuth(app);
SetupSwagger.UseSwagger(app);

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
    endpoints.MapHealthChecks("/health");
    endpoints.MapFallbackToFile("index.html");
});

app.UseSpaYarp();

app.Logger.LogInformation("Service started.");
SetupAudit.HttpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T>
}
