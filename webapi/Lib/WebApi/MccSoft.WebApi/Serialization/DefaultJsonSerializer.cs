using System.Text.Json;

namespace MccSoft.WebApi.Serialization;

public class DefaultJsonSerializer
{
    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(
            obj,
            SystemTextJsonSerializerSetup.GlobalSerializationOptions
        );
    }

    public static T Deserialize<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<T>(
            json,
            SystemTextJsonSerializerSetup.GlobalDeserializationOptions
        );
    }
}
