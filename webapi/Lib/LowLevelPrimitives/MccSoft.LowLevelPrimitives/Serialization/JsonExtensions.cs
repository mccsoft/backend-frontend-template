using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MccSoft.LowLevelPrimitives.Serialization;

public static class JsonExtensions
{
    public static string ToJsonString(this JsonDocument jsonDocument)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            jsonDocument.WriteTo(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static object GetValue(this JsonElement jsonElement)
    {
        return jsonElement.ValueKind switch
        {
            JsonValueKind.False => false,
            JsonValueKind.True => true,
            JsonValueKind.Number => jsonElement.GetDouble(),
            JsonValueKind.String => jsonElement.GetString(),
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            JsonValueKind.Object => jsonElement,
            JsonValueKind.Array => jsonElement.EnumerateArray().Select(x => x.GetValue()).ToArray(),
            _
                => throw new ArgumentOutOfRangeException(
                    nameof(jsonElement.ValueKind),
                    jsonElement.ValueKind,
                    "Unhandled ValueKind"
                )
        };
    }

    public static object GetValue(this JsonNode jsonNode)
    {
        if (jsonNode.AsValue().TryGetValue<bool>(out var boolResult))
            return boolResult;
        if (jsonNode.AsValue().TryGetValue<int>(out var intResult))
            return intResult;
        if (jsonNode.AsValue().TryGetValue<double>(out var doubleResult))
            return doubleResult;
        if (jsonNode.AsValue().TryGetValue<string>(out var stringResult))
            return stringResult;
        if (jsonNode is JsonArray jsonArray)
            return jsonArray.Select(x => x.GetValue()).ToList();
        if (jsonNode is JsonObject jsonObject)
            return jsonObject.Deserialize<object>();
        return jsonNode;
    }
}
