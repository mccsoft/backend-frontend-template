using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using MccSoft.WebApi.Domain.Helpers;
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
            context.Type == typeof(ValidationProblemDetails)
            || context.Type == typeof(ProblemDetails)
            || context.Type.IsAssignableTo(typeof(JsonDocument))
            || context.Type.IsAssignableTo(typeof(JsonNode))
        )
        {
            // there's no need to do detailed parsing of JToken types and system types
            return;
        }

        if (isPatchRequest && !_makePatchRequestFieldsNullable)
        {
            // this might be needed if some properties are decorated with [RequiredOrMissing] attribute
            // (to disallow setting null and empty values)
            foreach (var propertyKeyValue in schema.ActualProperties)
            {
                propertyKeyValue.Value.IsRequired = false;
            }
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
                property.Type
                is JsonObjectType.Object
                    or JsonObjectType.Array
                    or JsonObjectType.String
                    or JsonObjectType.Boolean
                    or JsonObjectType.Integer
                    or JsonObjectType.Number
                    or JsonObjectType.None /* enum */
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

                var propertyHasOppositeAttributes =
                    clrProperty.GetCustomAttribute<NotRequiredAttribute>() is { }
                    && clrProperty.GetCustomAttribute<RequiredAttribute>() is { };

                if (propertyHasOppositeAttributes)
                    throw new InvalidOperationException(
                        $"Property {clrProperty.Name} of class {context.Type.FullName} "
                            + $"has the opposite attributes: {nameof(NotRequiredAttribute)} and {nameof(RequiredAttribute)}"
                    );

                property.IsRequired =
                    clrProperty.GetCustomAttribute<RequiredAttribute>() is { }
                    || (
                        clrProperty.GetCustomAttribute<NotRequiredAttribute>() == null
                        && clrProperty.GetCustomAttribute<RequiredOrMissingAttribute>() == null
                        && !(isPatchRequest && _makePatchRequestFieldsNullable)
                    );
            }
        }
    }
}
