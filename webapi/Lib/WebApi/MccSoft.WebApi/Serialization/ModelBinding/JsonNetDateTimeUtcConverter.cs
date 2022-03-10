using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MccSoft.WebApi.Serialization.ModelBinding;

/// <summary>
/// Converter to set DateTimeKind.Utc for all parsed dates.
/// This is used to align with Utc everywhere strategy in PatientApp.
/// Based on suggestions from https://github.com/dotnet/runtime/issues/1566#issuecomment-745201271.
/// </summary>
public class JsonNetDateTimeUtcConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime());
    }
}
