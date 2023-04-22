using NSwag.Generation.AspNetCore;

namespace MccSoft.WebApi.Serialization.FromQueryJson;

public static class JsonQuerySwaggerGenExtensions
{
    /// <summary>
    /// Adds support for <see cref="FromJsonQueryAttribute"/> to NSwag.
    /// Integrate this by calling <see cref="JsonQuerySwaggerGenExtensions.AddJsonQuerySupport"/>:
    ///     builder.Services.AddOpenApiDocument(options => options.AddJsonQuerySupport());
    /// OpenApi spec: https://swagger.io/docs/specification/describing-parameters/#schema-vs-content
    /// </summary>
    public static AspNetCoreOpenApiDocumentGeneratorSettings AddJsonQuerySupport(
        this AspNetCoreOpenApiDocumentGeneratorSettings options
    )
    {
        options.OperationProcessors.Add(new JsonQueryNSwagOperationProcessor());
        return options;
    }
}
