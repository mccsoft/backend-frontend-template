using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MccSoft.WebApi.Serialization.DateTime
{
    /// <summary>
    /// Converter to be used on fields which should contain value that is the same across time zones.
    /// E.g. BirthDate (which should be the same in Tomsk and Berlin)
    /// </summary>
    public class IgnoreTimezoneDateTimeConverter : IsoDateTimeConverter
    {
        public const string Format = "yyyy-MM-ddTHH:mm:ss";

        public IgnoreTimezoneDateTimeConverter()
        {
            DateTimeFormat = Format;
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        ) {
            object baseValue = base.ReadJson(
                reader,
                typeof(DateTimeOffset?),
                existingValue,
                serializer
            );
            if (baseValue is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.DateTime;
            }

            return baseValue;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            // This override is needed when ServerTime = DatabaseTime = UTC.
            // Otherwise in that scenario timezone is added to response, what we try to avoid.
            if (value is System.DateTime dateTime)
            {
                value = new System.DateTime(dateTime.Ticks, DateTimeKind.Unspecified);
            }

            base.WriteJson(writer, value, serializer);
        }
    }
}
