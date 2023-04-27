using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MccSoft.LowLevelPrimitives.Serialization;

/// <summary>
/// Converter to set DateTimeKind.Utc for all parsed dates.
/// This is used to align with Utc everywhere strategy in PatientApp.
/// Works the same way as Newtonsoft.Json does with
/// <a href="https://www.newtonsoft.com/json/help/html/SerializeDateTimeZoneHandling.htm">DateTimeZoneHandling == Utc</a>
/// </summary>
public class JsonNetDateTimeUtcConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var dateTime = reader.GetDateTime();
        return dateTime.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => throw new UnreachableException()
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var dateTime = value.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => throw new UnreachableException()
        };

        writer.WriteStringValue(dateTime);
    }
}
