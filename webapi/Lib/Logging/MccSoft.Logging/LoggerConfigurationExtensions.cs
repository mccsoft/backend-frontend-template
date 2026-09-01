using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Destructurama;
using IdentityModel;
using MccSoft.HttpClientExtension;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Debugging;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;

namespace MccSoft.Logging;

public static class LoggerConfigurationExtensions
{
    public class SessionDetails { }

    public static void UseSerilog(this IApplicationBuilder app, IHostEnvironment hostingEnvironment)
    {
        if (!hostingEnvironment.IsEnvironment("Test"))
        {
            var excludePaths = app
                .ApplicationServices.GetRequiredService<IConfiguration>()
                .GetSection(SerilogRequestLoggingOptions.Position)
                .Get<SerilogRequestLoggingOptions>()
                ?.ExcludePaths;

            app.Use(
                async (httpContext, next) =>
                {
                    var requestProperties = GetRequestProperties(httpContext);

                    var logger =
                        httpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SessionDetails>>();
                    using (logger.BeginScope(requestProperties))
                    {
                        await next();
                    }
                }
            );

            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (context, elapsed, exception) =>
                {
                    var path = context.Request.Path.Value;

                    if (excludePaths?.Any(x => path.StartsWith(x)) is true)
                        return LogEventLevel.Verbose;

                    return LogEventLevel.Information;
                };
            });
        }
    }

    private static IReadOnlyList<KeyValuePair<string, object>> GetRequestProperties(
        HttpContext httpContext
    )
    {
        return
        [
            new("UserId", httpContext.User?.Identity?.GetClaimValueOrNull(JwtClaimTypes.Id)),
            new("ClientSession", httpContext.Request?.Headers["ClientSession"].ToString()),
            new("ClientVersion", httpContext.Request?.Headers["ClientVersion"].ToString()),
            new("ClientPlatform", httpContext.Request?.Headers["ClientPlatform"].ToString()),
            new("RequestHost", httpContext.Request.Host.Value),
            new("RequestScheme", httpContext.Request.Scheme),
            new("RemoteIpAddress", httpContext.Connection.RemoteIpAddress),
        ];
    }

    /// <summary>
    /// Configures Serilog sinks and enrichers used by the application.
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
            .Enrich.WithSpan()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("InstanceName", Environment.MachineName)
            .Enrich.WithProperty("Version", entryAssemblyVersion);

        loggerConfiguration.WriteToSentry(configuration);
        if (configuration.GetValue<bool>("Serilog:EnableFileOutput"))
            loggerConfiguration.WriteToFile(fileLogDirectory);
        loggerConfiguration.WriteToElasticSearch(configuration);
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
