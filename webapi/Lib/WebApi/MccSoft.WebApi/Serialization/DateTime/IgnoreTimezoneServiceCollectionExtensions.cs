using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MccSoft.WebApi.Serialization.DateTime
{
    public static class IgnoreTimezoneServiceCollectionExtensions
    {
        /// <summary>
        /// Sets up DateTime handling according to https://prismacode.visualstudio.com/Unicorn/_wiki/wikis/Unicorn.wiki/1163/DateTime-handling.
        /// Shortly, we assume that we only use DateTime type (never use DateTimeOffset) on backend, and all backend DateTime's are in UTC.
        /// To achieve that, we assume that Client always sends us dates in Client local timezone, with timezone specified
        /// (e.g. '2021-05-24T10:11:12+07:00').
        /// If Client wants to send us date only (e.g. for birthday field), it should send it with local timezone as well
        /// (e.g. '2021-05-24T00:00:00+07:00').
        /// On backend you should decorate such properties with [JsonConverter(typeof(IgnoreTimezoneDateTimeConverter))] attribute.
        /// </summary>
        public static IServiceCollection AddIgnoreTimezoneAttributes(
            [NotNull] this IServiceCollection services
        ) {
            services.AddControllers()
                .AddMvcOptions(
                    options =>
                    {
                        options.ModelBinderProviders.Insert(
                            0,
                            new IgnoreTimezoneDateTimeModelBinderProvider()
                        );
                    }
                )
                .AddNewtonsoftJson(
                    setupAction =>
                    {
                        JsonSerializerSettings settings = setupAction.SerializerSettings;
                        settings.Converters.Add(new DefaultDateTimeConverter());

                        // otherwise date is automatically converted, which will prevent us from handling TimezoneIndependentDate
                        settings.DateParseHandling = DateParseHandling.DateTimeOffset;
                    }
                );

            return services;
        }
    }
}
