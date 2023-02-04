using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MccSoft.WebApi.Serialization;

public class DefaultJsonSerializer
{
    /// <summary>
    /// This options could be used if you want to serialize something using the same settings configured for the App
    /// </summary>
    public static JsonSerializerOptions SerializationOptions { get; internal set; }

    /// <summary>
    /// This options could be used if you want to deserialize something using the same settings configured for the App
    /// </summary>
    public static JsonSerializerOptions DeserializationOptions { get; internal set; }

    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, SerializationOptions);
    }

    public static T Deserialize<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json))
            return null;
        if (typeof(T) == typeof(Dictionary<string, string>))
        {
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(
                json,
                DeserializationOptions
            );
            return result.ToDictionary(x => x.Key, x => x.Value.ToString()) as T;
        }
        if (typeof(T) == typeof(List<string>))
        {
            var result = JsonSerializer.Deserialize<List<object>>(json, DeserializationOptions);
            return result.Select(x => x.ToString()).ToList() as T;
        }

        return JsonSerializer.Deserialize<T>(json, DeserializationOptions);
    }
}
