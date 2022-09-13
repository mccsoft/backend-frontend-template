using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MccSoft.WebApi.Patching.Models;
using Microsoft.EntityFrameworkCore;
using NJsonSchema;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.TypeScript;
using Xunit;
using Xunit.Abstractions;
using System.Text;

namespace MccSoft.TemplateApp.ComponentTests
{
    public class BasicApiTests : TestBase
    {
        public BasicApiTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public async Task GetApiVersion_ReturnsVersionString()
        {
            var response = await Client.GetAsync("/api");
            response.EnsureSuccessStatusCode();
            var versionString = await response.Content.ReadAsStringAsync();

            var exception = Record.Exception(() => new Version(versionString));
            Assert.Null(exception);
        }

        [Fact]
        public void AllControllersAreResolvable()
        {
            var controllerTypes = typeof(Program).Assembly
                .GetTypes()
                .Where(
                    x =>
                        (
                            typeof(Microsoft.AspNetCore.Mvc.Controller).IsAssignableFrom(x)
                            || typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(x)
                        ) && !x.IsAbstract
                );

            using var scope = TestServer.Services.CreateScope();
            var errors = new StringBuilder();
            foreach (Type controllerType in controllerTypes)
            {
                // It will throw exception if any controller's dependency is not registered.`
                // (it doesn't require controllers themselves to be registered, as opposed to scope.GetService<T>())
                try
                {
                    ActivatorUtilities.CreateInstance(scope.ServiceProvider, controllerType);
                }
                catch (InvalidOperationException ex)
                {
                    errors.AppendLine(ex.Message);
                }
            }

            Assert.True(string.IsNullOrEmpty(errors.ToString()), errors.ToString());
        }

        /// <summary>
        /// Generates database structure as dgml-file for automated documentation.
        /// </summary>
        [Fact]
        public void GetDbStructure_Exported()
        {
            string exportPath = Configuration["Documentation:databaseExportPath"];
            Assert.False(
                string.IsNullOrEmpty(exportPath),
                "Export path for database schema export not found."
            );

            WithDbContextSync(context =>
            {
                string dgmlStr = context.AsDgml();
                File.WriteAllText(exportPath, dgmlStr);
            });
        }

        /// <summary>
        /// Generates HTTP Client for the API and places it in Lmt.Unicorn.Http project
        /// </summary>
        [Fact]
        public async Task GenerateHttpClient_ValidAndExported()
        {
            string exportPath = Path.GetTempFileName();
            string url = Configuration["Swagger:Endpoint:Url"];
            await SaveSwaggerJsonToFile(url, exportPath, null);
            await GenerateTypescriptHttpClient(exportPath);

            await SaveSwaggerJsonToFile(
                url.Replace("v1", "csharp"),
                exportPath,
                "https://localhost"
            );
            await GenerateCSharpHttpClient(exportPath);
        }

        private async Task SaveSwaggerJsonToFile(string url, string exportPath, string baseUrl)
        {
            Assert.False(string.IsNullOrEmpty(url), "No url for swagger found");
            Assert.False(string.IsNullOrEmpty(exportPath), "Export path for swagger not found.");

            HttpClient client = TestServer.CreateClient();
            client.Timeout = Client.Timeout;
            if (baseUrl != null)
            {
                client.BaseAddress = new Uri(baseUrl);
            }

            var response = await client.GetAsync(url);
            string swaggerJsonString = await response.Content.ReadAsStringAsync();
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                throw new Exception(swaggerJsonString, e);
            }
            JObject swagger = JObject.Parse(swaggerJsonString);
            //AssertEx.ApiDocumented(swagger);
            await using (StreamWriter file = File.CreateText(exportPath))
            {
                using JsonTextWriter writer = new JsonTextWriter(file);
                swagger.WriteTo(writer);
            }
        }

        private async Task GenerateCSharpHttpClient(string exportPath)
        {
            var document = await OpenApiDocument.FromJsonAsync(File.ReadAllText(exportPath));
            var csharpSettings = new CSharpClientGeneratorSettings
            {
                //UseBaseUrl = false,
                ParameterDateTimeFormat = "s",
                GenerateClientInterfaces = true,
                ClientBaseClass = "BaseClient",
                ExposeJsonSerializerSettings = true,
                GenerateUpdateJsonSerializerSettingsMethod = false,
                ClientBaseInterface = "IBaseClient",
                CSharpGeneratorSettings =
                {
                    Namespace = "MccSoft.TemplateApp.Http.Generated",
                    TemplateDirectory = "../../../../../src/MccSoft.TemplateApp.Http/template",
                }
            };

            var csharpGenerator = new CSharpClientGenerator(document, csharpSettings);
            var csharpCode = csharpGenerator.GenerateFile();

            // && yarn replace \", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore\" \" \"
            csharpCode = Regex.Replace(
                csharpCode,
                @", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore",
                " "
            );

            await File.WriteAllTextAsync(
                "../../../../../src/MccSoft.TemplateApp.Http/GeneratedClient.cs",
                csharpCode
            );
        }

        private async Task GenerateTypescriptHttpClient(string exportPath)
        {
            var document = await OpenApiDocument.FromJsonAsync(File.ReadAllText(exportPath));

            var typescriptSettings = new TypeScriptClientGeneratorSettings()
            {
                GenerateClientInterfaces = false,
                GenerateOptionalParameters = true,
                Template = TypeScriptTemplate.Axios,
                TypeScriptGeneratorSettings =
                {
                    TemplateDirectory =
                        "../../../../../../node_modules/react-query-swagger/templates",
                    NullValue = TypeScriptNullValue.Undefined,
                    MarkOptionalProperties = true,
                    GenerateConstructorInterface = true,
                    TypeStyle = TypeScriptTypeStyle.Class,
                }
            };
            var typeScriptClientGenerator = new TypeScriptClientGenerator(
                document,
                typescriptSettings
            );
            var typescriptCode = typeScriptClientGenerator.GenerateFile();

            // yarn replace "formatDate\\((.*?)\\) : <any>undefined" "formatDate($1) : $1" src/services/api/api-client.ts
            // && yarn replace \"this\\.(\\w*?)\\.toISOString\\(\\) : <any>undefined\" \"this.$1.toISOString() : this.$1\" app/api/api-client.ts
            // && yarn replace \"\\| undefined;\" \"| null;\"
            typescriptCode = Regex.Replace(
                typescriptCode,
                @"formatDate\((\w*?)\) : <any>undefined",
                "formatDate($1) : $1"
            );
            typescriptCode = Regex.Replace(
                typescriptCode,
                @"this\.(\w*?)\.toISOString\(\) : <any>undefined",
                "this.$1.toISOString() : this.$1"
            );
            typescriptCode = Regex.Replace(typescriptCode, @"\| undefined;", "| null;");
            typescriptCode = Regex.Replace(typescriptCode, @"""http://localhost""", @"""""");
            File.WriteAllText(
                "../../../../../../frontend/src/services/api/api-client.ts",
                typescriptCode
            );
        }

        /// <summary>
        /// Verifies that all DTOs inheriting from PatchRequest have only those properties
        /// that are present in the Entity they are patching.
        /// E.g. PatchPatientDto : PatchRequest<Patient> should only contain properties that are present in Patient.
        /// </summary>
        [Fact]
        public void PatchRequest_AllFieldsMatch()
        {
            var dtoTypes = typeof(Program).Assembly
                .GetTypes()
                .Where(x => IsSubclassOfRawGeneric(typeof(PatchRequest<>), x))
                .ToList();

            var exclusions = new Dictionary<Type, IList<string>>()
            {
                // { typeof(PatchPatientDto), new[] { nameof(PatchPatientDto.DeviceSerialNumber) } },
            };

            foreach (Type patchRequestType in dtoTypes)
            {
                var domainType = patchRequestType.BaseType!.GenericTypeArguments.FirstOrDefault();
                if (domainType == null)
                {
                    continue;
                }

                var properties = patchRequestType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.GetGetMethod() != null)
                    .Where(x => x.GetCustomAttribute<DoNotPatchAttribute>() == null);
                var domainProperties = domainType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .ToDictionary(x => x.Name);

                foreach (var property in properties)
                {
                    if (!domainProperties.ContainsKey(property.Name))
                    {
                        if (exclusions.TryGetValue(patchRequestType, out var excludedProperties))
                        {
                            if (excludedProperties.Contains(property.Name))
                            {
                                continue;
                            }
                        }

                        throw new ArgumentException(
                            $"{patchRequestType.Name}.{property.Name} is absent in domain class {domainType}"
                        );
                    }
                }
            }
        }

        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }
    }
}
