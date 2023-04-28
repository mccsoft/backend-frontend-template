#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MccSoft.LowLevelPrimitives.Serialization.JsonObject;

public class JsonObjectConverter : JsonConverter<System.Text.Json.Nodes.JsonObject>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert == typeof(System.Text.Json.Nodes.JsonObject);

    public override System.Text.Json.Nodes.JsonObject? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var jsonNode = JsonNode.Parse(ref reader);
        return jsonNode is null
            ? null
            : jsonNode as System.Text.Json.Nodes.JsonObject ?? throw new JsonException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        System.Text.Json.Nodes.JsonObject value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(
            writer,
            // Casting is required to prevent Stack overflow exception
            value as JsonNode,
            options
        );
    }
}
