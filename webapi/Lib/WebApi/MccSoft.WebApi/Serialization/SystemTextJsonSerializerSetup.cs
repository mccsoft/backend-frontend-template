using System.Text.Json;
using System.Text.Json.Serialization;
using MccSoft.WebApi.Patching;
using MccSoft.WebApi.Serialization.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.WebApi.Serialization;

public static partial class SystemTextJsonSerializerSetup
{
    /// <summary>
    /// Options that are used in <see cref="DefaultJsonSerializer" /> for serializing objects.
    /// They are meant to be the same as in request Body parsing.
    /// </summary>
    public static JsonSerializerOptions GlobalSerializationOptions { get; private set; }

    /// <summary>
    /// Options that are used in <see cref="DefaultJsonSerializer" /> for deserializing objects.
    /// They are meant to be the same as in request Body parsing.
    /// </summary>
    public static JsonSerializerOptions GlobalDeserializationOptions { get; private set; }

    public static JsonOptions SetupJson(this JsonOptions options)
    {
        options.JsonSerializerOptions.Converters.Add(new JsonNetDateTimeUtcConverter());
        options.JsonSerializerOptions.Converters.Add(new PatchRequestConverterFactory());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        CustomizeSettings(options.JsonSerializerOptions);

        GlobalSerializationOptions = options.JsonSerializerOptions;

        var deserializationOptions = new JsonSerializerOptions(options.JsonSerializerOptions);
        GlobalDeserializationOptions = deserializationOptions;

        return options;
    }

    static partial void CustomizeSettings(JsonSerializerOptions options);
}
