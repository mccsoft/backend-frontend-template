using System.Text.Json;
using MccSoft.LowLevelPrimitives.Serialization.JsonObject;

namespace MccSoft.WebApi.Serialization;

public partial class SystemTextJsonSerializerSetup
{
    static partial void CustomizeSettings(JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonObjectConverter());
    }
}
