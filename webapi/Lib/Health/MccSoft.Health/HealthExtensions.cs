using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using App.Metrics.AspNetCore.Health.Endpoints;
using App.Metrics.Health;
using App.Metrics.Health.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using App.Metrics.AspNetCore.Endpoints;

namespace MccSoft.Health
{
    /// <summary>
    ///     Health-related extensions to ASP.Net Core infrastructure.
    /// </summary>
    public static class HealthExtensions
    {
        /// <summary>
        ///     Adds the health endpoint services to the DI container.
        /// </summary>
        /// <param name="services">The service registry.</param>
        /// <param name="configuration">The application configuration.</param>
        public static void AddAppHealth(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            int? metricsPort = GetMetricsPort(configuration);
            IHealthRoot health = AppMetricsHealth
                .CreateDefaultBuilder()
                .HealthChecks.AddProcessPhysicalMemoryCheck(
                    "Working Set",
                    300_000_000,
                    degradedOnError: true
                )
                .Configuration.ReadFrom(configuration)
                .BuildAndAddTo(services);

            string healthRoute = configuration["HealthEndpointsOptions:HealthEndpointRoute"];
            if (!string.IsNullOrEmpty(healthRoute))
            {
                services.Configure(
                    (Action<HealthEndpointsHostingOptions>)(
                        opts =>
                        {
                            opts.HealthEndpoint = healthRoute;
                            opts.AllEndpointsPort = metricsPort;

                            // Set the port also to healthEndpoint. Because of bug in AppMetrics:
                            // https://github.com/AppMetrics/AspNetCoreHealth/issues/14
                            opts.HealthEndpointPort = metricsPort;
                        }
                    )
                );
            }

            services.Configure<HealthCheckSetting>(configuration.GetSection("HealthCheckSetting"));

            services.Configure(
                (Action<MetricsEndpointsHostingOptions>)(
                    opts =>
                    {
                        opts.AllEndpointsPort = metricsPort;
                    }
                )
            );

            services.AddHealth(health);
            services.AddHealthEndpoints(configuration);

            IMetricsRoot metrics = AppMetrics.CreateDefaultBuilder().Build();
            services.AddMetrics(metrics);
            services.AddMetricsEndpoints(
                configuration,
                opts =>
                {
                    opts.MetricsEndpointOutputFormatter =
                        new MetricsPrometheusTextOutputFormatter();
                }
            );
            services.AddMetricsTrackingMiddleware(
                // A workaround for 3xx treated as "unsuccessful", see
                // https://github.com/AppMetrics/AppMetrics/issues/516
                // Only partially fixes the problem, because 3xx responses are not counted in
                // application_httprequests_transactions_per_endpoint
                opts =>
                {
                    opts.IgnoredHttpStatusCodes = Enumerable.Range(300, 9).ToArray();
                }
            );
        }

        /// <summary>
        /// Wires the metric and health statistic collectors and endpoints into
        /// the Asp.Net pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public static void UseAppHealth(this IApplicationBuilder app)
        {
            app.UseHealthAllEndpoints();

            app.UseMetricsEndpoint();
            app.UseMetricsTextEndpoint();
            app.UseEnvInfoEndpoint();
            app.UseMetricsActiveRequestMiddleware();
            app.UseMetricsErrorTrackingMiddleware();
            app.UseMetricsPostAndPutSizeTrackingMiddleware();
            app.UseMetricsRequestTrackingMiddleware();
        }

        private static int? GetMetricsPort(IConfiguration configuration)
        {
            string urls = configuration["urls"];
            if (urls == null)
            {
                return null;
            }
            string[] splits = urls.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            int metricsPort = (
                from s in splits
                select new Uri(s.Replace("*", "localhost")).Port
            ).Last();

            return metricsPort;
        }
    }
}
