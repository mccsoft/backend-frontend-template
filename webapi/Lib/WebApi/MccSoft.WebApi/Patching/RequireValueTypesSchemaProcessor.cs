using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MccSoft.WebApi.Patching.Models;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using NJsonSchema.Generation;

namespace MccSoft.WebApi.Patching
{
    /// <summary>
    /// Schema processor that makes all value types (int, string, bool, etc.) required in OpenApi
    /// Classes that inherits from PatchRequest are omitted (since all properties in these classes are optional)
    /// </summary>
    public class RequireValueTypesSchemaProcessor : ISchemaProcessor
    {
        private static readonly Type _patchRequestType = typeof(IPatchRequest);

        public void Process(SchemaProcessorContext context)
        {
            var schema = context.Schema;
            if (
                _patchRequestType.IsAssignableFrom(context.Type)
                || context.Type == typeof(ValidationProblemDetails)
            ) {
                // Classes that inherits from PatchRequest are omitted (since all properties in these classes are optional)
                return;
            }

            Dictionary<string, PropertyInfo> clrProperties = context.Type.GetProperties()
                .ToDictionary(x => x.Name.ToLower());

            foreach (var propertyKeyValue in schema.Properties)
            {
                var property = propertyKeyValue.Value;
                string propertyName = property.Name;
                if (
                    property.Type == JsonObjectType.String
                    || property.Type == JsonObjectType.Boolean
                    || property.Type == JsonObjectType.Integer
                    || property.Type == JsonObjectType.Number
                    || property.Type == JsonObjectType.None /* enum */
                ) {
                    if (!schema.RequiredProperties.Contains(propertyName))
                    {
                        schema.RequiredProperties.Add(propertyName);
                    }

                    if (!clrProperties.ContainsKey(propertyName.ToLower()))
                    {
                        // this could happen with 'discriminator' field
                        continue;
                    }

                    PropertyInfo clrProperty = clrProperties[propertyName.ToLower()];
                    bool canBeNull =
                        clrProperty?.GetCustomAttribute<NotRequiredAttribute>() != null;

                    if (canBeNull)
                    {
                        if (schema.RequiredProperties.Contains(propertyName))
                        {
                            schema.RequiredProperties.Remove(propertyName);
                        }
                    }
                    else
                    {
                        if (!schema.RequiredProperties.Contains(propertyName))
                        {
                            schema.RequiredProperties.Add(propertyName);
                        }

                        if (property.Type == JsonObjectType.String)
                        {
                            if (property.Format != "date-time")
                            {
                                property.IsNullableRaw = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
