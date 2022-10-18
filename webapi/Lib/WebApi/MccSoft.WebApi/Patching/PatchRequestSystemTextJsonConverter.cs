using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MccSoft.WebApi.Patching.Models;

namespace MccSoft.WebApi.Patching;

/// <summary>
/// Class that plugs in to System.Text.Json deserialization pipeline for classes descending from <see cref="PatchRequest{TDomain}"/>.
/// For all properties, that are present in JSON it calls <see cref="PatchRequest.SetHasProperty"/>.`
/// </summary>
public class PatchRequestConverter : JsonConverter<IPatchRequest>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(IPatchRequest).IsAssignableFrom(typeToConvert);

    public override IPatchRequest Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        var patchRequest = (IPatchRequest)Activator.CreateInstance(typeToConvert)!;
        var properties = typeToConvert
            .GetProperties(
                BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.SetProperty
                    | BindingFlags.GetProperty
            )
            .ToDictionary(p => options.PropertyNamingPolicy?.ConvertName(p.Name) ?? p.Name);

        while (reader.Read())
            switch (reader.TokenType)
            {
                case JsonTokenType.EndObject:
                    return patchRequest;

                case JsonTokenType.PropertyName:
                    var property = properties[reader.GetString()!];
                    reader.Read();
                    property.SetValue(
                        patchRequest,
                        JsonSerializer.Deserialize(ref reader, property.PropertyType, options)
                    );
                    patchRequest.SetHasProperty(property.Name);
                    continue;
            }

        throw new JsonException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        IPatchRequest value,
        JsonSerializerOptions options
    ) => JsonSerializer.Serialize(writer, value, value.GetType(), options);
}
