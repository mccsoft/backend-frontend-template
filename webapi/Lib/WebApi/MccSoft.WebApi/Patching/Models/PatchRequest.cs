using System.Collections.Generic;
using NJsonSchema.Annotations;

namespace MccSoft.WebApi.Patching.Models
{
    /// <summary>
    /// Base class for DTOs used in PUT requests.
    /// Allows to differentiate between unset (absent) properties and properties explicitly set to NULL in http request.
    /// See <see cref="PatchRequestContractResolver"/> for details on how it is done
    /// </summary>
    [JsonSchemaIgnore]
    public abstract class PatchRequest<TDomain> : IPatchRequest
    {
        private Dictionary<string, bool> FieldStatus { get; set; } = new Dictionary<string, bool>();

        /// <summary>
        /// Returns true if property was present in http request; false otherwise
        /// </summary>
        public bool IsFieldPresent(string propertyName)
        {
            return FieldStatus.ContainsKey(propertyName.ToLowerInvariant());
        }

        public void SetHasProperty(string propertyName)
        {
            FieldStatus[propertyName.ToLowerInvariant()] = true;
        }
    }
}
