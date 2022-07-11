using MccSoft.LowLevelPrimitives.Serialization.DateOnlyConverters;
using MccSoft.WebApi.Patching;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MccSoft.WebApi.Serialization;

public partial class JsonSerializerSetup
{
    public static JsonSerializerSettings SetupJson(JsonSerializerSettings settings)
    {
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        settings.ContractResolver = new PatchRequestContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = false,
                OverrideSpecifiedNames = false
            }
        };
        settings.Converters.Add(new StringEnumConverter());
        settings.Converters.Add(new DateOnlyNewtonsoftConverter());
        settings.Converters.Add(new TimeOnlyNewtonsoftConverter());

        CustomizeSettings(settings);

        return settings;
    }

    static partial void CustomizeSettings(JsonSerializerSettings settings);
}
