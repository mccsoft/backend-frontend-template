using System.IO;
using MccSoft.Logging;
using MccSoft.WebApi.Sentry;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MccSoft.TemplateApp.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder<Startup>(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder<T>(string[] args) where T : class
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();

            return WebHost.CreateDefaultBuilder<T>(args)
                .UseConfiguration(config)
                .ConfigureAppConfiguration(
                    (builderContext, config) =>
                    {
                        config.AddJsonFile("appsettings.local.json", true);
                    }
                )
                .UseSerilog(
                    (hostingContext, loggerConfiguration) =>
                    {
                        loggerConfiguration.ConfigureSerilog(
                            hostingContext.HostingEnvironment,
                            hostingContext.Configuration
                        );
                    }
                );
        }
    }
}
