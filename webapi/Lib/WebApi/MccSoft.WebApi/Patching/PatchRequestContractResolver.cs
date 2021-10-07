using System.Reflection;
using MccSoft.WebApi.Patching.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MccSoft.WebApi.Patching
{
    /// <summary>
    /// Class that plugs in to Newtonsoft deserialization pipeline for classes descending from <see cref="PatchRequest{TDomain}"/>.
    /// For all properties, that are present in JSON it calls <see cref="PatchRequest.SetHasProperty"/>.`
    /// </summary>
    public class PatchRequestContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization
        ) {
            var prop = base.CreateProperty(member, memberSerialization);

            prop.SetIsSpecified += (o, o1) =>
            {
                if (o is IPatchRequest patchRequest)
                {
                    patchRequest.SetHasProperty(prop.PropertyName);
                }
            };

            return prop;
        }
    }
}
