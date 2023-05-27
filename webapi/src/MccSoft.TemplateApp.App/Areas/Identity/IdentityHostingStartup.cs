using MccSoft.TemplateApp.App.Areas.Identity;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]

namespace MccSoft.TemplateApp.App.Areas.Identity;

public class IdentityHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) => { });
    }
}
