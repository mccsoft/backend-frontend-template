using MccSoft.LowLevelPrimitives;
using MccSoft.TemplateApp.App.Features.Products;
using MccSoft.TemplateApp.App.Services.Authentication;
using MccSoft.TemplateApp.App.Services.Authentication.Seed;
using MccSoft.TemplateApp.App.Utils;

namespace MccSoft.TemplateApp.App;

public static class SetupServices
{
    public static void AddServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        services
            .AddScoped<IDateTimeProvider, DateTimeProvider>()
            .AddTransient<IUserAccessor, UserAccessor>()
            .AddScoped<DefaultUserSeeder>()
            .AddScoped<ProductService>();
    }
}
