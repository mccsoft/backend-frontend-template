using System.Reflection;
using System.Text.Json;
using MccSoft.TemplateApp.App.Settings;
using MccSoft.WebApi.Patching;
using MccSoft.WebApi.Serialization.FromQueryJson;
using MccSoft.WebApi.Swagger;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Examples;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors.Security;

namespace MccSoft.TemplateApp.App.Setup;

public static partial class SetupSwagger
{
    private static SwaggerOptions GetSwaggerOptions(this IConfiguration configuration)
    {
        return configuration.GetSection("Swagger").Get<SwaggerOptions>();
    }

    public static void AddSwagger(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var swaggerOptions = configuration.GetSwaggerOptions();
        services.AddExampleProviders(Assembly.GetEntryAssembly());
        services.AddOpenApiDocument(
            (options, provider) =>
            {
                options.AddExamples(provider);
                options.AddJsonQuerySupport();
                ConfigureOpenApiDocument(options, swaggerOptions);
                options.SchemaSettings.SchemaProcessors.Add(
                    new RequireValueTypesSchemaProcessor(makePatchRequestFieldsNullable: false)
                );
                options.SchemaSettings.SchemaProcessors.Add(
                    new ValidationProblemDetailsSchemaProcessor()
                );
            }
        );
        services.AddOpenApiDocument(options =>
        {
            ConfigureOpenApiDocument(options, swaggerOptions);
            options.DocumentName = "csharp";
            options.SchemaSettings.SchemaProcessors.Add(
                new RequireValueTypesSchemaProcessor(makePatchRequestFieldsNullable: true)
            );
            options.SchemaSettings.SchemaProcessors.Add(
                new ValidationProblemDetailsSchemaProcessor()
            );
        });

        // If you'd like to modify this class, consider adding your custom code in the SetupSwagger.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        AddProjectSpecifics(builder);
    }

    private static void ConfigureOpenApiDocument(
        AspNetCoreOpenApiDocumentGeneratorSettings options,
        SwaggerOptions swaggerOptions
    )
    {
        options.AddSecurity(
            "Bearer",
            new OpenApiSecurityScheme()
            {
                Type = OpenApiSecuritySchemeType.OpenIdConnect,
                OpenIdConnectUrl = "/.well-known/openid-configuration",
                Flow = OpenApiOAuth2Flow.Application,
            }
        );
        options.AddSecurity(
            "JWT Token",
            new OpenApiSecurityScheme()
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                Description = "Copy 'Bearer ' + valid JWT token into field",
                In = OpenApiSecurityApiKeyLocation.Header,
            }
        );

        options.PostProcess = document =>
        {
            document.Info = new OpenApiInfo
            {
                Version = swaggerOptions.Version,
                Title = swaggerOptions.Title,
                Description = swaggerOptions.Description,
                Contact = new OpenApiContact { Email = swaggerOptions.Contact.Email },
                License = new OpenApiLicense { Name = swaggerOptions.License.Name },
            };
        };
        options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
        // maps JsonDocument to `{ [key: string]: any; }`
        options.SchemaSettings.TypeMappers.Add(
            new PrimitiveTypeMapper(
                typeof(JsonDocument),
                s =>
                {
                    s.Type = JsonObjectType.Object;
                    s.AdditionalPropertiesSchema = JsonSchema.CreateAnySchema();
                }
            )
        );

        options.SchemaSettings.GenerateEnumMappingDescription = true;

        AdjustDefaultOpenApiDocument(options);
    }

    public static void UseSwagger(WebApplication app)
    {
        var swaggerOptions = app.Configuration.GetSwaggerOptions();
        if (!swaggerOptions.Enabled)
        {
            return;
        }

        app.UseOpenApi(options =>
        {
            options.Path = "/swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUi(options =>
        {
            options.Path = "/swagger";
            options.OAuth2Client = new OAuth2ClientSettings()
            {
                AppName = "swagger",
                Realm = "swagger",
                ClientId = "web_client",
                ClientSecret = "",
                UsePkceWithAuthorizationCodeGrant = true,
            };
        });

        // If you'd like to modify this class, consider adding your custom code in the SetupSwagger.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        UseProjectSpecifics(app);
    }

    static partial void AddProjectSpecifics(WebApplicationBuilder builder);

    static partial void UseProjectSpecifics(IApplicationBuilder app);

    static partial void AdjustDefaultOpenApiDocument(
        AspNetCoreOpenApiDocumentGeneratorSettings options
    );
}
