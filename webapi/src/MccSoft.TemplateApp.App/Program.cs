using MccSoft.DomainHelpers.DomainEvents.Events;
using MccSoft.Logging;
using MccSoft.Mailing;
using MccSoft.PersistenceHelpers.DomainEvents;
using MccSoft.TemplateApp.App;
using MccSoft.TemplateApp.App.DomainEventHandlers;
using MccSoft.TemplateApp.App.Setup;
using MccSoft.TemplateApp.Domain;
using MccSoft.WebApi.Sentry;
using MccSoft.WebApi.Serialization.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MccSoft.TemplateApp.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connectionString =
    builder.Configuration.GetConnectionString("TemplateAppDbContextConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'TemplateAppDbContextConnection' not found."
    );

builder.Services.AddDbContext<TemplateAppDbContext>(
    options => options.UseSqlServer(connectionString)
);
;

builder.Services
    .AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<TemplateAppDbContext>();
;

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

app.UseHttpsRedirection();

SetupLocalization.UseLocalization(app);
SetupAspNet.UseFrontlineServices(app);

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
    // Expose the Program class for use with WebApplicationFactory<T>
}
