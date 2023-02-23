using MccSoft.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable UnusedMember.Global

namespace MccSoft.Testing;

/// <summary>
/// Provides custom assertion helpers for tests.
/// </summary>
public static class AssertEx
{
    /// <summary>
    /// Types which should be ignored in the definition checks.
    /// These types are from the framework and can not be documented by us.
    /// </summary>
    private static readonly HashSet<string> _ignoreDtos = new HashSet<string>
    {
        "ProblemDetails",
        "DayOfWeek"
    };

    /// <summary>
    /// Validates if all documentation related information are defined in swagger.json.
    /// Like: Endpoints-Method, -Parameters,-Result and Dtos.
    /// </summary>
    /// <param name="swagger">The JObject of the swagger.json file.</param>
    public static void ApiDocumented(JObject swagger)
    {
        var errorBuilder = new StringBuilder();
        ValidateEndpoints(swagger, errorBuilder);
        ValidateDtos(swagger, errorBuilder);

        // string errorStr = errorBuilder.ToString();
        // Assert.True(string.IsNullOrEmpty(errorStr), errorStr);
    }

    /// <summary>
    /// Validates if all documentation related information are defined for the type.
    /// Like: ClassName
    /// </summary>
    /// <param name="messageTypes">List of types which should be checked</param>
    /// <returns>The checked types</returns>
    public static List<TypeDescription> TypesDocumented(List<Type> messageTypes)
    {
        var typeList = new List<TypeDescription>();
        var libDocument = new Dictionary<string, XmlDocument>();

        string GetClassDocumentation(string documentationPath, string className)
        {
            if (!libDocument.TryGetValue(documentationPath, out XmlDocument document))
            {
                Assert.True(
                    File.Exists(documentationPath),
                    $"The Lib: '{className}' has no xml-description"
                );

                document = new XmlDocument();
                document.Load(documentationPath);
                libDocument[documentationPath] = document;
            }

            string path = $"T:{className}";
            XmlNode summaryNode = document.SelectSingleNode($"//member[@name='{path}']/summary");
            string description = summaryNode?.InnerText.Trim();
            if (description != null)
            {
                description = Regex.Replace(description, @"\s+", " ");
            }

            return description;
        }

        foreach (Type messageType in messageTypes)
        {
            Type typeForDoc = messageType.IsGenericType
                ? messageType.GenericTypeArguments.First()
                : messageType;

            string messageName = typeForDoc.FullName;
            string libLocation = typeForDoc.Assembly.Location;
            string docuPath =
                libLocation.Substring(
                    0,
                    libLocation.LastIndexOf(".", StringComparison.InvariantCulture)
                ) + ".xml";

            string description = GetClassDocumentation(docuPath, messageName);

            Assert.False(
                string.IsNullOrWhiteSpace(description),
                $"The Message: '{messageName}' has no description"
            );

            typeList.Add(new TypeDescription(messageName, description));
        }

        return typeList;
    }

    /// <summary>
    /// Assert if the given message has the given field and value.
    /// </summary>
    public static void HasFieldAndValue(LoggedMessage message, Field field, object expectedValue)
    {
        object value = HasField(message, field);
        Assert.Equal(expectedValue, value);
    }

    /// <summary>
    /// Assert if the given message has the given field.
    /// </summary>
    /// <returns>The value of the given field</returns>
    public static object HasField(LoggedMessage message, Field field)
    {
        KeyValuePair<string, object> keyValue = Assert.Single(
            message.LogValues,
            kvp => $"{{{kvp.Key}}}" == field.ToString()
        );
        return keyValue.Value;
    }

    private static void ValidateDtos(JObject swagger, StringBuilder errorBuilder)
    {
        dynamic swaggerDynamic = swagger;
        dynamic definitions = swaggerDynamic.components?.schemas;
        if (definitions == null)
        {
            return;
        }
        foreach (dynamic definition in definitions)
        {
            JProperty definitionProp = definition;
            string dtoName = definitionProp.Name;
            if (_ignoreDtos.Contains(dtoName))
            {
                continue;
            }

            if (!IsPropertySet(definitionProp.Value, "description"))
            {
                errorBuilder.AppendLine($"The class {dtoName} has no description");
            }

            JToken properties = definitionProp.Value["properties"];
            // In case of a enum we have no properties
            if (properties != null)
            {
                foreach (JProperty propertyProp in properties.Children<JProperty>())
                {
                    // Validates that a property have a description.
                    string propertyName = propertyProp.Name;
                    if (propertyProp.Value["$ref"] != null)
                    {
                        // Ignore the check of items with ref property,
                        // because these items are have a reference to another class
                        // and this class will be checked as own object.
                        continue;
                    }

                    if (!IsPropertySet(propertyProp.Value, "description"))
                    {
                        errorBuilder.AppendLine(
                            $"The Property : '{propertyName}' of the class: '{dtoName}' "
                                + "has no description"
                        );
                    }
                }
            }
        }
    }

    private static void ValidateEndpoints(JObject swagger, StringBuilder errorBuilder)
    {
        dynamic swaggerDynamic = swagger;
        dynamic paths = swaggerDynamic.paths;
        foreach (dynamic path in paths)
        {
            JProperty pathProp = path;
            string endpoint = pathProp.Name;

            foreach (JProperty httpMethodProp in pathProp.Value.Children<JProperty>())
            {
                // Validates that a controller end-point have a description
                string httpMethod = httpMethodProp.Name;
                if (!IsPropertySet(httpMethodProp.Value, "summary"))
                {
                    errorBuilder.AppendLine(
                        $"The http-method: '{httpMethod}' of endpoint: '{endpoint}' has no summary"
                    );
                }

                JToken parameters = httpMethodProp.Value["parameters"];
                if (parameters != null)
                {
                    foreach (JToken parameterToken in parameters.Children<JToken>())
                    {
                        // Validates that all parameters of a end-point have a description
                        string parameterName = parameterToken["name"].Value<string>();
                        if (!IsPropertySet(parameterToken, "description"))
                        {
                            errorBuilder.AppendLine(
                                $"The Parameter: '{parameterName}' of http-method: "
                                    + $"'{httpMethod}' of endpoint: '{endpoint}' has no description"
                            );
                        }
                    }
                }

                JToken responses = httpMethodProp.Value["responses"];
                foreach (JProperty responseProp in responses.Children<JProperty>())
                {
                    // Validates that the return type of the end-point have a description.
                    string responseCode = responseProp.Name;

                    if (!IsPropertySet(responseProp.Value, "description"))
                    {
                        errorBuilder.AppendLine(
                            $"The response-code: '{responseCode}' of http-method: "
                                + $"'{httpMethod}' of endpoint: '{endpoint}' has no description"
                        );
                    }
                }
            }
        }
    }

    private static bool IsPropertySet(JToken jObj, string propertyName)
    {
        JToken token = jObj[propertyName];
        return token != null && !string.IsNullOrEmpty(token.Value<string>());
    }
}
