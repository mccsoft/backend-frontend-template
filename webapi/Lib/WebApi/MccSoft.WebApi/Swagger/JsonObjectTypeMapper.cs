using System;
using System.Text.Json.Nodes;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;

namespace MccSoft.WebApi.Swagger;

/// <summary>
/// Maps <see cref="JsonObject"/> to <c>`{ [key: string]: any; }`</c>
/// </summary>
public class JsonObjectTypeMapper : ITypeMapper
{
    public void GenerateSchema(JsonSchema schema, TypeMapperContext context)
    {
        schema.Type = JsonObjectType.Object;
        schema.AdditionalPropertiesSchema = JsonSchema.CreateAnySchema();
    }

    public Type MappedType => typeof(JsonObject);
    public bool UseReference => false;
}
