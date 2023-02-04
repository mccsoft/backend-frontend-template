using System.Text.Json;
using System.Text.Json.Serialization;
using MccSoft.WebApi.Patching;
using MccSoft.WebApi.Serialization.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace MccSoft.WebApi.Serialization;

public static partial class SystemTextJsonSerializerSetup
{
    public static JsonOptions SetupJson(this JsonOptions options)
    {
        options.JsonSerializerOptions.Converters.Add(new JsonNetDateTimeUtcConverter());
        options.JsonSerializerOptions.Converters.Add(new PatchRequestConverterFactory());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        CustomizeSettings(options.JsonSerializerOptions);

        DefaultJsonSerializer.SerializationOptions = options.JsonSerializerOptions;

        var deserializationOptions = new JsonSerializerOptions(options.JsonSerializerOptions);
        DefaultJsonSerializer.DeserializationOptions = deserializationOptions;

        return options;
    }

    static partial void CustomizeSettings(JsonSerializerOptions options);
}
