using System.Collections.Generic;
using System.Text.Json.JsonDiffPatch;

namespace MccSoft.LowLevelPrimitives.Serialization.JsonObject;

public class JsonObjectEqualityComparer : IEqualityComparer<System.Text.Json.Nodes.JsonObject>
{
    private readonly JsonElementComparison _jsonElementComparison;

    public JsonObjectEqualityComparer() { }

    public JsonObjectEqualityComparer(JsonElementComparison jsonElementComparison)
    {
        _jsonElementComparison = jsonElementComparison;
    }

    public bool Equals(System.Text.Json.Nodes.JsonObject x, System.Text.Json.Nodes.JsonObject y) =>
        x.DeepEquals(y, _jsonElementComparison);

    public int GetHashCode(System.Text.Json.Nodes.JsonObject obj) => obj.GetHashCode();

    /// <inheritdoc cref="JsonElementComparison.RawText"/>
    public static readonly JsonObjectEqualityComparer RowText = new(JsonElementComparison.RawText);

    /// <inheritdoc cref="JsonElementComparison.Semantic"/>
    public static readonly JsonObjectEqualityComparer Semantic =
        new(JsonElementComparison.Semantic);
}
