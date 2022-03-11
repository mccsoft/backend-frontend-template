using System;
using System.Globalization;
using Newtonsoft.Json;

namespace MccSoft.LowLevelPrimitives.Serialization.DateOnlyConverters;

public class DateOnlyNewtonsoftConverter : Newtonsoft.Json.JsonConverter<DateOnly?>
{
    private const string DateFormat = "yyyy-MM-dd";

    public override DateOnly? ReadJson(
        JsonReader reader,
        Type objectType,
        DateOnly? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var value = reader.Value;
        if (value == null)
            return null;

        if (value is DateTime dateTime)
            return new DateOnly(dateTime.Date.Year, dateTime.Date.Month, dateTime.Date.Day);

        return DateOnly.ParseExact((string)value, DateFormat, CultureInfo.InvariantCulture);
    }

    public override void WriteJson(JsonWriter writer, DateOnly? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString(DateFormat, CultureInfo.InvariantCulture));
    }
}

public class TimeOnlyNewtonsoftConverter : Newtonsoft.Json.JsonConverter<TimeOnly?>
{
    private const string TimeFormat = "HH:mm:ss.FFFFFFF";

    public override TimeOnly? ReadJson(
        JsonReader reader,
        Type objectType,
        TimeOnly? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var value = reader.Value;
        if (value == null)
            return null;
        return TimeOnly.ParseExact((string)value, TimeFormat, CultureInfo.InvariantCulture);
    }

    public override void WriteJson(JsonWriter writer, TimeOnly? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString(TimeFormat, CultureInfo.InvariantCulture));
    }
}
