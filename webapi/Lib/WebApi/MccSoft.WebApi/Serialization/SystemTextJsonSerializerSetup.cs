using System.Text.Json;
using MccSoft.WebApi.Serialization.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.WebApi.Serialization;

public static partial class SystemTextJsonSerializerSetup
{
    /// <summary>
    /// This options could be used if you want to serialize/deserialize something using the same settings configured for the App
    /// </summary>
    public static JsonSerializerOptions GlobalSerializationOptions { get; private set; }

    public static JsonOptions SetupJson(this JsonOptions options)
    {
        options.UseDateOnlyTimeOnlyStringConverters();
        options.JsonSerializerOptions.Converters.Add(new JsonNetDateTimeUtcConverter());

        CustomizeSettings(options.JsonSerializerOptions);

        GlobalSerializationOptions = options.JsonSerializerOptions;

        return options;
    }

    static partial void CustomizeSettings(JsonSerializerOptions options);
}
