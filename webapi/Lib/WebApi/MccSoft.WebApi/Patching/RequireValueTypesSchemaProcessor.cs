using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using MccSoft.WebApi.Patching.Models;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using NJsonSchema.Generation;

namespace MccSoft.WebApi.Patching;

/// <summary>
/// Schema processor that makes all value types (int, string, bool, etc.) required in OpenApi
/// (for example, in `components/schema` node of swagger.json :
/// "ProductDto": {
///   "type": "object",
///   "required": [  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
///     "title"      // This block is missing without this Processor
///   ],             // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
///   "properties": {
///     "title": {
///       "type": "string"
///     }
///   }
/// }
///
/// Classes that inherits from PatchRequest are omitted (since all properties in these classes are optional)
/// </summary>
public class RequireValueTypesSchemaProcessor : ISchemaProcessor
{
    private NullabilityInfoContext _nullabilityContext = new();
    private readonly bool _makePatchRequestFieldsNullable;
    private static readonly Type _patchRequestType = typeof(IPatchRequest);

    public RequireValueTypesSchemaProcessor(bool makePatchRequestFieldsNullable)
    {
        _makePatchRequestFieldsNullable = makePatchRequestFieldsNullable;
    }

    public void Process(SchemaProcessorContext context)
    {
        bool isPatchRequest = _patchRequestType.IsAssignableFrom(context.Type);

        var schema = context.Schema;
        if (
            (isPatchRequest && !_makePatchRequestFieldsNullable)
            || context.Type == typeof(ValidationProblemDetails)
            || context.Type == typeof(ProblemDetails)
        )
        {
            // Classes that inherits from PatchRequest are omitted (since all properties in these classes are optional)
            return;
        }

        if (context.Type.IsAssignableTo(typeof(JsonDocument)))
        {
            // there's no need to do detailed parsing of JToken types
            return;
        }

        Dictionary<string, PropertyInfo> clrProperties;
        try
        {
            clrProperties = context.Type.GetProperties().ToDictionary(x => x.Name.ToLower());
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Error getting properties from type {context.Type}",
                e
            );
        }

        foreach (var propertyKeyValue in schema.ActualProperties)
        {
            JsonSchemaProperty property = propertyKeyValue.Value;
            string propertyName = property.Name;
            if (
                property.Type == JsonObjectType.String
                || property.Type == JsonObjectType.Boolean
                || property.Type == JsonObjectType.Integer
                || property.Type == JsonObjectType.Number
                || property.Type == JsonObjectType.None /* enum */
            )
            {
                if (!clrProperties.ContainsKey(propertyName.ToLower()))
                {
                    // this could happen with 'discriminator' field
                    continue;
                }

                var clrProperty = clrProperties[propertyName.ToLower()];
                var nullabilityInfo = _nullabilityContext.Create(clrProperty);

                var propIsNullable =
                    isPatchRequest && _makePatchRequestFieldsNullable
                    || nullabilityInfo.WriteState is NullabilityState.Nullable;

                if (propIsNullable)
                    property.IsNullableRaw = true;

                property.IsRequired =
                    clrProperty.GetCustomAttribute<NotRequiredAttribute>() == null
                    && !(isPatchRequest && _makePatchRequestFieldsNullable);
            }
        }
    }
}
