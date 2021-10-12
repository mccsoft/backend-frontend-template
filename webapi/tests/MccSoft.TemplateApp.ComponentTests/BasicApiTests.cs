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
using MccSoft.TemplateApp.App;
using MccSoft.TemplateApp.Http.Generated;
using MccSoft.Testing;
using MccSoft.WebApi.Patching.Models;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.TypeScript;
using Xunit;

namespace MccSoft.TemplateApp.ComponentTests
{
    public class BasicApiTests : TestBase
    {
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
            var controllerTypes = typeof(Startup).Assembly.GetTypes()
                .Where(
                    x =>
                        (
                            typeof(Microsoft.AspNetCore.Mvc.Controller).IsAssignableFrom(x)
                            || typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(x)
                        ) && !x.IsAbstract
                );

            using var scope = TestServer.Host.Services.CreateScope();
            foreach (Type controllerType in controllerTypes)
            {
                // It will throw exception if any controller's dependency is not registered.
                // (it doesn't require controllers themselves to be registered, as opposed to scope.GetService<T>())
                ActivatorUtilities.CreateInstance(scope.ServiceProvider, controllerType);
            }
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

            WithDbContext(
                context =>
                {
                    string dgmlStr = context.AsDgml();
                    File.WriteAllText(exportPath, dgmlStr);
                }
            );
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
                CSharpGeneratorSettings = { Namespace = "MccSoft.TemplateApp.Http.Generated", }
            };

            var csharpGenerator = new CSharpClientGenerator(document, csharpSettings);
            var csharpCode = csharpGenerator.GenerateFile();

            // && yarn replace \", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore\" \" \"
            csharpCode = Regex.Replace(
                csharpCode,
                @", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore",
                " "
            );

            File.WriteAllText(
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
                        "../../../../../../node_modules/nswag-react-query/templates",
                    NullValue = TypeScriptNullValue.Undefined,
                    MarkOptionalProperties = true,
                    GenerateConstructorInterface = true,
                    TypeStyle = TypeScriptTypeStyle.Class,
                }
            };
            var typeScriptClientGenerator = new TypeScriptClientGenerator(
                document,
                typescriptSettings
            ) {

            };
            var typescriptCode = typeScriptClientGenerator.GenerateFile();

            // && yarn replace \"this\\.(\\w*?)\\.toISOString\\(\\) : <any>undefined\" \"this.$1.toISOString() : this.$1\" app/api/api-client.ts
            // && yarn replace \"\\| undefined;\" \"| null;\"
            typescriptCode = Regex.Replace(
                typescriptCode,
                @"this\.(\w*?)\.toISOString\(\) : <any>undefined",
                "this.$1.toISOString() : this.$1"
            );
            typescriptCode = Regex.Replace(typescriptCode, @"\| undefined;", "| null;");
            typescriptCode = Regex.Replace(typescriptCode, @"""http://localhost""", @"""""");
            // File.WriteAllText("../../../../../../@MccSoft.unicorn-shared/app/api/api-client.ts",
            //     typescriptCode);
        }

        /// <summary>
        /// Verifies that all DTOs inheriting from PatchRequest have only those properties
        /// that are present in the Entity they are patching.
        /// E.g. PatchPatientDto : PatchRequest<Patient> should only contain properties that are present in Patient.
        /// </summary>
        [Fact]
        public void PatchRequest_AllFieldsMatch()
        {
            var dtoTypes = typeof(Startup).Assembly.GetTypes()
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

                var properties = patchRequestType.GetProperties(
                        BindingFlags.Instance | BindingFlags.Public
                    )
                    .Where(x => x.GetGetMethod() != null)
                    .Where(x => x.GetCustomAttribute<DoNotPatchAttribute>() == null);
                var domainProperties = domainType.GetProperties(
                        BindingFlags.Instance | BindingFlags.Public
                    )
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
