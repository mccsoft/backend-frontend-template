using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using NSwag;
using NSwag.Generation.AspNetCore;
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
        if (context is AspNetCoreOperationProcessorContext aspNetOperationContext)
        {
            var parameterDescriptions = aspNetOperationContext.ApiDescription.ParameterDescriptions;
            var parameters = aspNetOperationContext.OperationDescription.Operation.Parameters;
            for (int i = 0; i < parameterDescriptions.Count; i++)
            {
                var parameterDescription = parameterDescriptions[i];
                if (parameterDescription.ModelMetadata is DefaultModelMetadata defaultModelMetadata)
                {
                    if (
                        defaultModelMetadata.Attributes.Attributes.Any(
                            attribute => attribute.GetType() == typeof(FromJsonQueryAttribute)
                        )
                    )
                    {
                        var openApiParameter = parameters[i];
                        var content = new Dictionary<string, OpenApiMediaType>()
                        {
                            [MediaTypeNames.Application.Json] = new OpenApiMediaType()
                            {
                                Schema = openApiParameter.Schema,
                            }
                        };
                        openApiParameter.ExtensionData ??= new Dictionary<string, object>();
                        openApiParameter.ExtensionData.Add("content", content);
                        // We need to add schema somewhere where it could be parsed by
                        // JsonSchemaReferenceUtilities.UpdateSchemaReferencePaths.
                        openApiParameter.AdditionalItemsSchema = openApiParameter.Schema;
                        openApiParameter.Schema = null;
                    }
                }
            }
        }

        return true;
    }
}
