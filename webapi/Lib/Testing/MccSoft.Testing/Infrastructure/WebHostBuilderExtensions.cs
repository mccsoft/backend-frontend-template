using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MccSoft.Testing.Infrastructure;

public static class WebHostBuilderExtensions
{
    /// <summary>
    /// This is just a copy from default ConfigureAppConfiguration,
    /// except that reloadOnChange is set to false.
    /// It is required to be run in azure-pipelines on Ubuntu Latest
    /// (otherwise it causes
    /// System.IO.IOException : The configured user limit (128) on the number of inotify instances has been reached, or the per-process limit on the number of open file descriptors has been reached.
    ///)
    /// </summary>
    /// <param name="builder"></param>
    public static void AddJsonFileConfigurationWithoutReloadOnChange(this IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (hostingContext, config) =>
            {
                config.Sources.Clear();

                IWebHostEnvironment env = hostingContext.HostingEnvironment;

                config
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddJsonFile(
                        $"appsettings.{env.EnvironmentName}.json",
                        optional: true,
                        reloadOnChange: false
                    );
                config.AddJsonFile("appsettings.local.json", true, reloadOnChange: false);
            }
        );
    }
}
