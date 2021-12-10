using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Destructurama;
using IdentityModel;
using MccSoft.HttpClientExtension;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Loggly;

namespace MccSoft.Logging
{
    public static class LoggerConfigurationExtensions
    {
        public static void UseSerilog(
            this IApplicationBuilder app,
            IHostEnvironment hostingEnvironment
        )
        {
            if (!hostingEnvironment.IsEnvironment("Test"))
            {
                app.UseSerilogRequestLogging(
                    options =>
                    {
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
                    }
                );
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
            IWebHostEnvironment hostingEnvironment,
            IConfiguration configuration,
            string fileLogDirectory = "logs"
        )
        {
            string sentryDsn = configuration.GetValue<string>("Sentry:Dsn");
            RemoteLoggerOptions remoteLoggerOption = configuration
                .GetSection("Serilog:Remote")
                .Get<RemoteLoggerOptions>();

            // By default DateTime in messages is formatted as "10/25/2020 00:00:00", so we create a custom formatter.
            var isoCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            isoCulture.DateTimeFormat = new DateTimeFormatInfo
            {
                ShortDatePattern = "yyyy-MM-dd",
                LongTimePattern = "HH:mm:ss"
            };

            // Write errors that happen in the logger itself to the stderr and to a file, to enable two use cases:
            // 1. Immediately see lost (rejected by ES) messages in the output of `docker service logs service_name`.
            // 2. To keep errors related to lost messages in a file between service restarts.
            SelfLog.Enable(
                msg =>
                {
                    Console.Error.WriteLine(msg);
                }
            );

            // Entry assembly is service App project, so version should be correct. If you'll replace it with
            // GetExecutingAssembly then you'll get version of Lmt.Logging library.
            AssemblyName entryAssembly = Assembly.GetEntryAssembly()?.GetName();
            string entryAssemblyVersion = entryAssembly?.Version?.ToString(fieldCount: 3) ?? "";
            string serviceName = entryAssembly?.Name ?? "backend";

            loggerConfiguration.Destructure
                .JsonNetTypes()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty(
                    "InstanceName",
                    remoteLoggerOption.InstanceName ?? Environment.MachineName
                )
                .Enrich.WithProperty("Version", entryAssemblyVersion);

            if (!string.IsNullOrEmpty(sentryDsn))
            {
                loggerConfiguration.WriteTo.Logger(
                    lc =>
                        lc.ExcludeValidationErrors()
                            .WriteTo.Sentry(
                                s =>
                                {
                                    s.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                                    s.MinimumEventLevel = LogEventLevel.Error;
                                    s.Dsn = sentryDsn;
                                }
                            )
                );
            }

            loggerConfiguration.WriteTo.Logger(
                lc =>
                    lc.ExcludeEfInformation()
                        .WriteTo.File(
                            new CompactJsonFormatter(),
                            $"{fileLogDirectory.TrimEnd('/')}/{serviceName}.txt",
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 100 * 1000 * 1000,
                            rollOnFileSizeLimit: true
                        )
            );

            // If the stage is not local development stage - push logs to Elasticsearch.
            if (!string.IsNullOrEmpty(remoteLoggerOption?.Server))
            {
                loggerConfiguration.WriteTo.Logger(
                    lc =>
                        lc.ExcludeEfInformation()
                            .WriteTo.Loggly(
                                formatProvider: isoCulture,
                                logglyConfig: new LogglyConfiguration()
                                {
                                    CustomerToken = remoteLoggerOption.Token,
                                    ApplicationName = remoteLoggerOption.InstanceName,
                                    EndpointPort = remoteLoggerOption.Port,
                                    EndpointHostName = remoteLoggerOption.Server,
                                }
                            )
                );
            }

            if (hostingEnvironment.IsDevelopment())
            {
                loggerConfiguration.WriteTo.Console(formatProvider: isoCulture);
            }
            else
            {
                // On a stage we write only errors and warnings to the console to quickly see problems when doing
                // `docker service logs service_name`.
                loggerConfiguration.WriteTo.Logger(
                    lc => lc.MinimumLevel.Warning().WriteTo.Console(formatProvider: isoCulture)
                );
            }
        }

        private static LoggerConfiguration ExcludeEfInformation(
            this LoggerConfiguration loggerConfiguration
        )
        {
            return loggerConfiguration.Filter.ByExcluding(
                le =>
                    le.Level == LogEventLevel.Information
                    && Matching.FromSource("Microsoft.EntityFrameworkCore")(le)
            );
        }

        private static LoggerConfiguration ExcludeValidationErrors(
            this LoggerConfiguration loggerConfiguration
        )
        {
            return loggerConfiguration.Filter.ByExcluding(
                le => le.Exception is ValidationException || le.Exception is INoSentryException
            );
        }
    }
}
