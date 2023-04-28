using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MccSoft.Testing;
using MccSoft.WebApi.Patching.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MccSoft.TemplateApp.ComponentTests;

public partial class BasicApiTests : ComponentTestBase
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
        string openApiFilePath = Path.GetTempFileName();
        string outputDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(outputDirPath);
        try
        {
            string url = Configuration["Swagger:Endpoint:Url"];
            await SaveSwaggerJsonToFile(url, openApiFilePath, null);
            await GenerateTypescriptHttpClient(openApiFilePath, outputDirPath);

            await SaveSwaggerJsonToFile(
                url.Replace("v1", "csharp"),
                openApiFilePath,
                "https://localhost"
            );
            await GenerateCSharpHttpClient(openApiFilePath, outputDirPath);
        }
        finally
        {
            File.Delete(openApiFilePath);
            Directory.Delete(outputDirPath, true);
        }
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
        var swagger = JObject.Parse(swaggerJsonString);
        AssertEx.ApiDocumented(swagger);
        File.WriteAllText(exportPath, swaggerJsonString);
    }

    private async Task GenerateCSharpHttpClient(string inputPath, string outputDirPath)
    {
        await RunScriptFromFrontendFolder(
            $"yarn nswag openapi2csclient /input:{inputPath} /output:{outputDirPath}/GeneratedClient.cs /templateDirectory:../webapi/src/MccSoft.TemplateApp.Http/template /namespace:MccSoft.TemplateApp.Http.Generated /generateClientInterfaces:true /clientBaseClass:BaseClient /exposeJsonSerializerSettings:true /generateUpdateJsonSerializerSettingsMethod:false /clientBaseInterface:IBaseClient /jsonLibrary:SystemTextJson"
        );
        await RunScriptFromFrontendFolder(
            $"yarn replace ', NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore' ' ' {outputDirPath}/GeneratedClient.cs"
        );
    }

    private async Task GenerateTypescriptHttpClient(string inputPath, string outputDir)
    {
        await RunScriptFromFrontendFolder(
            $"yarn react-query-swagger openapi2tsclient /tanstack /input:{inputPath} /output:{outputDir}/api-client.ts /template:Axios /serviceHost:. /minimal"
        );
    }

    private async Task RunScriptFromFrontendFolder(string script)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Environment.OSVersion.Platform == PlatformID.Win32NT ? "powershell" : "node",
            // Arguments = $"yarn generate-api-client-axios /input:{exportPath}",
            Arguments = script,
            WorkingDirectory = "../../../../../../frontend",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true, };

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                OutputHelper.WriteLine(JsonConvert.SerializeObject(args.Data));
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                OutputHelper.WriteLine(JsonConvert.SerializeObject(args.Data));
        };

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var taskCompletionSource = new TaskCompletionSource<bool>();

        // process.WaitForExit() causes infinite wait
        // and i don't want to set a long timeout
        process.Exited += (_, _) =>
        {
            process.CancelOutputRead();
            process.CancelErrorRead();

            var exitCode = process.ExitCode;

            process.Dispose();

            if (exitCode != 0)
            {
                OutputHelper.WriteLine(
                    "Running tool \'yarn\': exit code is not zero, but \'{exitCode}\'"
                );
            }
            taskCompletionSource.SetResult(exitCode == 0);
        };

        await taskCompletionSource.Task;
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
