using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
            .AddControllers(options => options.UseDateOnlyTimeOnlyStringConverters())
            .AddJsonOptions(opts =>
            {
                opts.UseDateOnlyTimeOnlyStringConverters();
                opts.JsonSerializerOptions.Converters.Add(new JsonNetDateTimeUtcConverter());
            })
            .AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                opts.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                opts.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            });

        HttpClientDefaults
            .MediaTypeFormatters
            .JsonFormatter
            .SerializerSettings
            .DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        JsonConvert.DefaultSettings = () =>
        {
            var serializerSettings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return serializerSettings;
        };
    }
}
