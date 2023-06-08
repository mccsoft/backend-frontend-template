using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MccSoft.WebApi.Patching.Models;

namespace MccSoft.WebApi.Patching;

public class PatchRequestConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(IPatchRequest).IsAssignableFrom(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new PatchRequestConverter(typeToConvert);
    }
}

/// <summary>
/// Class that plugs in to System.Text.Json deserialization pipeline for classes descending from <see cref="PatchRequest{TDomain}"/>.
/// For all properties, that are present in JSON it calls <see cref="PatchRequest.SetHasProperty"/>.`
/// </summary>
public class PatchRequestConverter : JsonConverter<IPatchRequest>
{
    private readonly Type _type;

    public override bool CanConvert(Type typeToConvert) =>
        typeof(IPatchRequest).IsAssignableFrom(typeToConvert);

    internal PatchRequestConverter(Type type)
    {
        _type = type;
    }

    public override IPatchRequest Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        var patchRequest = (IPatchRequest)Activator.CreateInstance(_type)!;
        var properties = _type
            .GetProperties(
                BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.SetProperty
                    | BindingFlags.GetProperty
            )
            .ToDictionary(
                p =>
                    options.PropertyNamingPolicy?.ConvertName(p.Name)?.ToLower() ?? p.Name.ToLower()
            );

        while (reader.Read())
            switch (reader.TokenType)
            {
                case JsonTokenType.EndObject:
                    return patchRequest;

                case JsonTokenType.PropertyName:
                    var propertyName = reader.GetString()!;
                    var propertyNameLowercase = propertyName.ToLower();

                    reader.Read();

                    if (properties.TryGetValue(propertyNameLowercase, out var property))
                    {
                        if (reader.TokenType == JsonTokenType.Null)
                        {
                            var isPropertyNullable =
                                Nullable.GetUnderlyingType(property.PropertyType) != null
                                || !property.PropertyType.IsValueType;

                            if (!isPropertyNullable)
                            {
                                var requiredAttribute = new RequiredAttribute();

                                throw new ValidationException(
                                    new ValidationResult(
                                        requiredAttribute.FormatErrorMessage(propertyName),
                                        new[] { propertyName }
                                    ),
                                    requiredAttribute,
                                    null
                                );
                            }
                        }
                        var value = JsonSerializer.Deserialize(
                            ref reader,
                            property.PropertyType,
                            options
                        );

                        property.SetValue(patchRequest, value);
                        patchRequest.SetHasProperty(property.Name);
                    }
                    else
                    {
                        JsonSerializer.Deserialize(ref reader, typeof(object), options);
                    }
                    continue;
                case JsonTokenType.None:
                case JsonTokenType.StartObject:
                case JsonTokenType.StartArray:
                case JsonTokenType.EndArray:
                case JsonTokenType.Comment:
                case JsonTokenType.String:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                case JsonTokenType.Null:
                default:
                    throw new ArgumentOutOfRangeException();
            }

        throw new JsonException();
    }

    private static JsonSerializerOptions? _writeOptions = null;
    private static object _writeLockObject = new();

    public override void Write(
        Utf8JsonWriter writer,
        IPatchRequest value,
        JsonSerializerOptions options
    )
    {
        if (_writeOptions == null)
        {
            lock (_writeLockObject)
            {
                if (_writeOptions == null)
                {
                    _writeOptions = new(options);
                    _writeOptions.Converters.Remove(
                        _writeOptions.Converters.First(x => x is PatchRequestConverterFactory)
                    );
                }
            }
        }
        JsonSerializer.Serialize(writer, value, value.GetType(), _writeOptions);
    }
}
