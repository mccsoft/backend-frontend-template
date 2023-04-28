using MccSoft.WebApi.Swagger;
using NSwag.Generation.AspNetCore;

namespace MccSoft.TemplateApp.App.Setup;

public static partial class SetupSwagger
{
    static partial void AddProjectSpecifics(WebApplicationBuilder builder) { }

    static partial void UseProjectSpecifics(IApplicationBuilder app) { }

    static partial void AdjustDefaultOpenApiDocument(
        AspNetCoreOpenApiDocumentGeneratorSettings options
    )
    {
        options.TypeMappers.Add(new JsonObjectTypeMapper());
    }
}
