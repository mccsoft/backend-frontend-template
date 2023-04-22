using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MccSoft.WebApi.Serialization.FromQueryJson;

/// <summary>
/// Adds support for <see cref="FromJsonQueryAttribute"/> to NSwag.
/// Integrate this by calling <see cref="JsonQuerySwaggerGenExtensions.AddJsonQuerySupport"/>:
///     builder.Services.AddOpenApiDocument(options => options.AddJsonQuerySupport());
/// Taken from https://abdus.dev/posts/aspnetcore-model-binding-json-query-params/
/// OpenApi spec: https://swagger.io/docs/specification/describing-parameters/#schema-vs-content
/// </summary>
public class JsonQueryNSwagOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        foreach (
            (ParameterInfo parameterInfo, OpenApiParameter openApiParameter) in context.Parameters
        )
        {
            var fromJsonQueryAttribute = parameterInfo.CustomAttributes.FirstOrDefault(
                attribute => attribute.AttributeType == typeof(FromJsonQueryAttribute)
            );
            if (fromJsonQueryAttribute == null)
                continue;
            var content = new Dictionary<string, OpenApiMediaType>()
            {
                [MediaTypeNames.Application.Json] = new OpenApiMediaType()
                {
                    Schema = openApiParameter.Schema,
                }
            };
            openApiParameter.ExtensionData.Add("content", content);
            // We need to add schema somewhere where it could be parsed by
            // JsonSchemaReferenceUtilities.UpdateSchemaReferencePaths.
            openApiParameter.AdditionalItemsSchema = openApiParameter.Schema;
            openApiParameter.Schema = null;
        }
        return true;
    }
}
