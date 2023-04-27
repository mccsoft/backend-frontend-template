using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema.Generation;

namespace MccSoft.WebApi.Swagger;

/// <summary>
/// For some reason property "errors" of ValidationProblemDetails in a generated OpenAPI
/// is not required but the same property in base class HttpValidationProblemDetails
/// is required.
/// It causes a type check error for api-client and this processor fixes it
/// </summary>
public class ValidationProblemDetailsSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (context.Type != typeof(ValidationProblemDetails))
            return;

        var errorsProperty = context.Schema.ActualProperties
            .Select(x => x.Value)
            .FirstOrDefault(
                x => x.Name.ToLower() == nameof(ValidationProblemDetails.Errors).ToLower()
            );

        if (errorsProperty is null)
            return;

        errorsProperty.IsRequired = true;
        errorsProperty.IsNullableRaw = false;
    }
}
