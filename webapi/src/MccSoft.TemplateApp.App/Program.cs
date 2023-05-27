using MccSoft.Logging;
using MccSoft.Mailing;
using MccSoft.PersistenceHelpers.DomainEvents;
using MccSoft.TemplateApp.App;
using MccSoft.TemplateApp.App.DomainEventHandlers;
using MccSoft.TemplateApp.App.Setup;
using MccSoft.WebApi.Sentry;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsEnvironment("Test"))
{
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
}

SetupDatabase.AddDatabase(builder);
SetupAudit.ConfigureAudit(builder.Services, builder.Configuration);
builder.Services.AddDomainEventsWithMediatR(typeof(Program), typeof(LogDomainEventHandler));

SetupAuth.ConfigureAuth(builder);
SetupLocalization.AddLocalization(builder);

builder.Services.AddMailing(builder.Configuration.GetSection("Email"));

SetupAspNet.AddAspNet(builder);

SetupSwagger.AddSwagger(builder);

// builder.Services.AddWebHooks();

// Set up your application-specific services here
SetupServices.AddServices(builder.Services, builder.Configuration, builder.Environment);

// ---------------------------------
//
var app = builder.Build();

// ---------------------------------


app.UseSerilog(app.Environment);
app.Logger.LogSentryTestError("TemplateApp");

await SetupDatabase.RunMigration(app);

app.UseHttpsRedirection();

SetupAspNet.UseFrontlineServices(app);
SetupLocalization.UseLocalization(app);

SetupHangfire.UseHangfire(app);

SetupAuth.UseAuth(app);
SetupSwagger.UseSwagger(app);

SetupAspNet.UseEndpoints(app);
SetupAudit.UseAudit(app);

app.Logger.LogInformation("Service started");

app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T> in tests
}
