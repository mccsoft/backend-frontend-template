using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Destructurama;
using IdentityModel;
using MccSoft.HttpClientExtension;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Loggly;

namespace MccSoft.Logging;

public static class LoggerConfigurationExtensions
{
    public static void UseSerilog(this IApplicationBuilder app, IHostEnvironment hostingEnvironment)
    {
        if (!hostingEnvironment.IsEnvironment("Test"))
        {
            var excludePaths = app
                .ApplicationServices.GetRequiredService<IConfiguration>()
                .GetSection(SerilogRequestLoggingOptions.Position)
                .Get<SerilogRequestLoggingOptions>()
                ?.ExcludePaths;

            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (context, elapsed, exception) =>
                {
                    var path = context.Request.Path.Value;

                    if (excludePaths?.Any(x => path.StartsWith(x)) is true)
                        return LogEventLevel.Verbose;

                    return LogEventLevel.Information;
                };

                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set(
                        "UserId",
                        httpContext.User?.Identity?.GetClaimValueOrNull(JwtClaimTypes.Id)
                    );
                    diagnosticContext.Set(
                        "ClientSession",
                        httpContext.Request?.Headers["ClientSession"].ToString()
                    );
                    diagnosticContext.Set(
                        "ClientVersion",
                        httpContext.Request?.Headers["ClientVersion"].ToString()
                    );
                    diagnosticContext.Set(
                        "ClientPlatform",
                        httpContext.Request?.Headers["ClientPlatform"].ToString()
                    );
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set(
                        "RemoteIpAddress",
                        httpContext.Connection.RemoteIpAddress
                    );
                };
            });
        }
    }

    /// <summary>
    /// Configures Serilog to use Elasticsearch, and enriches with default global log context.
    /// </summary>
    /// <param name="loggerConfiguration">Serilog default configuration.</param>
    /// <param name="hostingEnvironment">Hosting environment</param>
    /// <param name="configuration">Service configuration.</param>
    /// <param name="fileLogDirectory">Path to folder to write file logs to</param>
    public static void ConfigureSerilog(
        this LoggerConfiguration loggerConfiguration,
        IHostEnvironment hostingEnvironment,
        IConfiguration configuration,
        string fileLogDirectory = "logs"
    )
    {
        // Write errors that happen in the logger itself to the stderr and to a file, to enable two use cases:
        // 1. Immediately see lost (rejected by ES) messages in the output of `docker service logs service_name`.
        // 2. To keep errors related to lost messages in a file between service restarts.
        SelfLog.Enable(msg =>
        {
            Console.Error.WriteLine(msg);
        });

        AssemblyName entryAssembly = Assembly.GetEntryAssembly()?.GetName();
        string entryAssemblyVersion = entryAssembly?.Version?.ToString(fieldCount: 3) ?? "";

        loggerConfiguration
            .Destructure.JsonNetTypes()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("InstanceName", Environment.MachineName)
            .Enrich.WithProperty("Version", entryAssemblyVersion);

        loggerConfiguration.WriteToSentry(configuration);
        loggerConfiguration.WriteToElasticSearch(configuration);
        if (configuration.GetValue<bool>("Serilog:EnableFileOutput"))
            loggerConfiguration.WriteToFile(fileLogDirectory);

        if (
            hostingEnvironment.IsDevelopment()
            || hostingEnvironment.IsEnvironment("Test")
            || configuration.GetValue<bool>("Serilog:EnableConsoleOutput")
        )
        {
            loggerConfiguration.WriteTo.Console(formatProvider: GetFormatProvider());
        }
        else
        {
            // On a stage we write only errors and warnings to the console to quickly see problems when doing
            // `docker service logs service_name`.
            loggerConfiguration.WriteTo.Logger(lc =>
                lc.MinimumLevel.Warning().WriteTo.Console(formatProvider: GetFormatProvider())
            );
        }
    }

    private static void WriteToFile(
        this LoggerConfiguration loggerConfiguration,
        string fileLogDirectory
    )
    {
        loggerConfiguration.WriteTo.Logger(lc =>
            lc.ExcludeEfInformation()
                .WriteTo.File(
                    new CompactJsonFormatter(),
                    $"{fileLogDirectory.TrimEnd('/')}/TemplateApp.txt",
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 100 * 1000 * 1000,
                    rollOnFileSizeLimit: true
                )
        );
    }

    private static void WriteToSentry(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration
    )
    {
        string sentryDsn = configuration.GetValue<string>("Sentry:Dsn");
        if (!string.IsNullOrEmpty(sentryDsn))
        {
            loggerConfiguration.WriteTo.Logger(lc =>
                lc.ExcludeValidationErrors()
                    .WriteTo.Sentry(s =>
                    {
                        s.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                        s.MinimumEventLevel = LogEventLevel.Error;
                        s.Dsn = sentryDsn;
                        s.AttachStacktrace = true;
                        s.Environment = configuration.GetValue<string>("General:SiteUrl");
                    })
            );
        }
    }

    internal static LoggerConfiguration ExcludeEfInformation(
        this LoggerConfiguration loggerConfiguration
    )
    {
        // EF logs are too noisy even on Information level.
        return loggerConfiguration.Filter.ByExcluding(le =>
            le.Level == LogEventLevel.Information
            && Matching.FromSource("Microsoft.EntityFrameworkCore")(le)
        );
    }

    private static LoggerConfiguration ExcludeValidationErrors(
        this LoggerConfiguration loggerConfiguration
    )
    {
        return loggerConfiguration.Filter.ByExcluding(le =>
            le.Exception is ValidationException || le.Exception is INoSentryException
        );
    }

    internal static CultureInfo GetFormatProvider()
    {
        // By default DateTime in messages is formatted as "10/25/2020 00:00:00", so we create a custom formatter.
        var isoCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        isoCulture.DateTimeFormat = new DateTimeFormatInfo
        {
            ShortDatePattern = "yyyy-MM-dd",
            LongTimePattern = "HH:mm:ss",
        };
        return isoCulture;
    }
}
