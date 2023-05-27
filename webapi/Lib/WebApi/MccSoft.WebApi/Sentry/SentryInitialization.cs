using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentry;
using Sentry.AspNetCore;
using Sentry.Extensibility;

namespace MccSoft.WebApi.Sentry;

public static class SentryInitialization
{
    /// <summary>
    ///     Configures asp.net to send exceptions to Sentry server.
    ///     Server is determined by Sentry__Dsn environment variable.
    ///     This variable is null on developer's machines, so nothing is sent to Sentry.
    /// </summary>
    public static IWebHostBuilder UseAppSentry(
        this IWebHostBuilder builder,
        Action<SentryAspNetCoreOptions> configureOptions = null,
        bool requestBodyBufferingIsEnabled = true
    )
    {
        builder.ConfigureServices(
            services => services.AddTransient<ISentryEventProcessor, SentryEventProcessor>()
        );
        builder.UseSentry(options =>
        {
            options.Debug = true;
            options.AttachStacktrace = true;
            options.MinimumBreadcrumbLevel = LogLevel.Debug;

            // In ApiGateway we need to turn off request body buffering, because
            // of the Ocelot issue with EnableBuffering() in Sentry middleware,
            // see: https://github.com/ThreeMammals/Ocelot/issues/792
            // In other cases than None,
            // body of the request will be trimmed if request is too big.
            options.MaxRequestBodySize = requestBodyBufferingIsEnabled
                ? RequestSize.Medium
                : RequestSize.None;

            // Sentry errors (most important - failure to send an event) will be reported
            // as usual logs to our logstash.
            options.DiagnosticLevel = SentryLevel.Error;

            configureOptions?.Invoke(options);
        });

        return builder;
    }
}
