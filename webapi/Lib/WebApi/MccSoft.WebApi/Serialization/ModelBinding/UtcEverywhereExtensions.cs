using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MccSoft.WebApi.Serialization.ModelBinding;

public static class UtcEverywhereExtensions
{
    /// <summary>
    /// Configures the Utc everywhere strategy, meaning all DateTime's are with DateTimeKind.Utc on backend.
    /// </summary>
    /// <param name="services"></param>
    public static void AddUtcEverywhere(this IServiceCollection services)
    {
        services.AddMvc(options =>
        {
            options.ModelBinderProviders.Insert(0, new UtcDateTimeModelBinderProvider());
        });

        services
            .AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonNetDateTimeUtcConverter());
            });

        HttpClientDefaults
            .MediaTypeFormatters
            .JsonFormatter
            .SerializerSettings
            .DateTimeZoneHandling = DateTimeZoneHandling.Utc;
    }
}
