using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Newtonsoft.Json;

namespace MccSoft.WebApi.Serialization.DateTime
{
    /// <summary>
    /// Provider that instructs ASP.Net Core to use <see cref="IgnoreTimezoneDateTimeModelBinder"/>
    /// for GET properties decorated with [JsonConverter(typeof(IgnoreTimezoneDateTimeConverter))]
    /// </summary>
    public class IgnoreTimezoneDateTimeModelBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var modelType = context.Metadata.UnderlyingOrModelType;
            if (modelType == typeof(System.DateTime))
            {
                if (context.Metadata is DefaultModelMetadata defaultModelMetadata)
                {
                    var attributes =
                        defaultModelMetadata.Attributes.ParameterAttributes
                        ?? defaultModelMetadata.Attributes.PropertyAttributes;
                    if (
                        attributes?.OfType<JsonConverterAttribute>()
                            .Any(x => x.ConverterType == typeof(IgnoreTimezoneDateTimeConverter))
                        == true
                    ) {
                        return new IgnoreTimezoneDateTimeModelBinder();
                    }
                }
            }

            return null;
        }
    }
}
