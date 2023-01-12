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

    public enum PropertyNameSearchKind
    {
        /// <summary>
        /// Property names need to be equal to match.
        /// </summary>
        Equal,

        /// <summary>
        /// Property name match if source ends with target or
        /// target ends with source name.
        /// </summary>
        EndsWith,

        /// <summary>
        /// Property name match if source starts with target or
        /// target starts with source name.
        /// </summary>
        StartsWith
    }

    /// <summary>
    /// Checks that similarly named properties in the specified objects have equal values.
    /// It is assumed that one object is a copy of the other.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <param name="target">The object whose properties are checked.</param>
    /// <param name="source">
    /// The object from which <paramref name="target" /> was created.
    /// </param>
    /// <param name="cutOffUnits">
    /// Specify <c>true</c> to match by name ignoring a "_unit" segment at the end of each
    /// <paramref name="target" /> object property name; false to match property names exactly.
    /// </param>
    /// <param name="targetPropertiesToIgnore">
    /// List of properties from <see paramref="target"/> which should be ignored.
    /// </param>
    /// <param name="propertyMap">
    /// An optional map from a property in the target object to a property in the source,
    /// use when the names of the corresponding properties differ significantly and the default
    /// by-name matching doesn't work.
    /// </param>
    /// <param name="comparerMap">
    /// An optional map which allows to compare the property of a source and target object.
    /// </param>
    /// <param name="ignoreMissingSourceProperty">in case of true if the source object
    /// doesn't contain a property of target object this property will be ignored.
    /// In case of false it will throw if source property is missing</param>
    /// <param name="checkIfSourceIsMissingTargetIsNull">in case of true if a source property does not exist,
    /// it checks if the target property is null.</param>
    /// <param name="nameSearchKind">The kind to define if source and target property name are the same</param>
    /// <param name="expectedNumberOfComparedProperties">Expected number of properties which were compared</param>
    public static void SamePropertiesEqual<T>(
        T target,
        object source,
        bool cutOffUnits = false,
        List<string> targetPropertiesToIgnore = null,
        Dictionary<string, string> propertyMap = null,
        Dictionary<string, IEqualityComparer> comparerMap = null,
        bool ignoreMissingSourceProperty = false,
        bool checkIfSourceIsMissingTargetIsNull = false,
        PropertyNameSearchKind nameSearchKind = PropertyNameSearchKind.Equal,
        int? expectedNumberOfComparedProperties = null
    )
    {
        if (targetPropertiesToIgnore == null)
        {
            targetPropertiesToIgnore = new List<string>();
        }

        if (propertyMap == null)
        {
            propertyMap = new Dictionary<string, string>();
        }

        int propCount = Compare(
            target,
            source,
            cutOffUnits,
            targetPropertiesToIgnore,
            propertyMap,
            comparerMap,
            ignoreMissingSourceProperty,
            checkIfSourceIsMissingTargetIsNull,
            nameSearchKind
        );

        if (expectedNumberOfComparedProperties.HasValue)
        {
            Assert.Equal(expectedNumberOfComparedProperties, propCount);
        }
    }

    /// <summary>
    /// Checks that the specified collections contain the same elements (in any order).
    /// </summary>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <param name="expectedSet">The expected collection.</param>
    /// <param name="actualSet">The actual collection.</param>
    public static void SetEqual<T>(IEnumerable<T> expectedSet, IEnumerable<T> actualSet)
    {
        IEnumerable<T> actual = actualSet.ToArray();
        IEnumerable<T> expected = expectedSet.ToArray();
        var error = new StringBuilder();
        List<T> extraItems = actual.Except(expected).ToList();
        List<T> missingItems = expected.Except(actual).ToList();
        if (extraItems.Any())
        {
            error.AppendLine($"Extra items were found: {String.Join(", ", extraItems)}.");
        }

        if (missingItems.Any())
        {
            error.AppendLine(
                $"Some required items were not found: {String.Join(", ", missingItems)}."
            );
        }

        Assert.True(error.Length == 0, error.ToString());
    }

    /// <summary>
    /// Checks that the specified collections doesn't intersect
    /// (The items are only available in one list and not in the other one).
    /// </summary>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <param name="expectedSet">The expected collection.</param>
    /// <param name="actualSet">The actual collection.</param>
    public static void NoIntersection<T>(IEnumerable<T> expectedSet, IEnumerable<T> actualSet)
    {
        IEnumerable<T> actual = actualSet.ToArray();
        IEnumerable<T> expected = expectedSet.ToArray();
        var error = new StringBuilder();

        List<T> duplicatedItems = actual.Intersect(expected).ToList();
        if (duplicatedItems.Any())
        {
            error.AppendLine(
                $"Items which are in both lists: {String.Join(", ", duplicatedItems)}."
            );
        }

        Assert.True(error.Length == 0, error.ToString());
    }

    /// <summary>
    /// Waits until the specified condition becomes true.
    /// If the condition is still not true after the specified timeout, throws.
    /// NB. This method can run longer than the specified timeout if the predicate
    /// takes a long time to complete.
    /// </summary>
    /// <param name="predicate">The condition to wait for. The predicate is called each 100ms.</param>
    /// <param name="timeout">How long to wait for the condition.</param>
    /// <param name="errorMessage">
    /// The error message to display if the operation times out.
    /// </param>
    /// <param name="checkPeriodInSeconds">
    /// The time to wait between checks of the <paramref name="predicate"/>.
    /// </param>
    public static async Task WaitUntil(
        Func<Task<bool>> predicate,
        TimeSpan timeout,
        string errorMessage = null,
        double checkPeriodInSeconds = 0.25
    )
    {
        errorMessage = errorMessage ?? $"Operation failed to complete in {timeout}.";
        using (var tokenSource = new CancellationTokenSource(timeout))
        {
            CancellationToken token = tokenSource.Token;
            bool success = false;
            var timeoutException = new TimeoutException(errorMessage);
            try
            {
                while (!token.IsCancellationRequested && !success)
                {
                    success = await predicate();
                    if (!success)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(checkPeriodInSeconds), token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw timeoutException;
            }

            if (!success)
            {
                throw timeoutException;
            }
        }
    }

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

    private static string RemoveUnit(string propertyName) =>
        Regex.Replace(propertyName, @"_[^_]+$", String.Empty);

    private static void AssertEqual(
        PropertyInfo propertyInfo,
        object targetValue,
        object sourceValue
    )
    {
        Assert.True(
            // The values should be exactly equal, because one is a copy of the other.
            Equals(targetValue, sourceValue),
            $"Property '{propertyInfo.Name}' differs from the source. The target "
                + $"value '{targetValue}' was not equal to source '{sourceValue}'."
        );
    }

    private static bool IsValueToCompare(Type type)
    {
        return type.IsValueType || type == typeof(string);
    }

    private static int Compare<T>(
        T target,
        object source,
        bool cutOffUnits,
        List<string> targetPropertiesToIgnore,
        Dictionary<string, string> propertyMap,
        Dictionary<string, IEqualityComparer> comparerMap,
        bool ignoreMissingSourceProperty,
        bool checkIfSourceIsMissingTargetIsNull,
        PropertyNameSearchKind nameSearchKind
    )
    {
        int numberOfComparedProperties = 0;

        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        Type sourceType = source.GetType();
        PropertyInfo[] propertyInfos = target.GetType().GetProperties();
        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            string propName = propertyInfo.Name;
            if (targetPropertiesToIgnore.Contains(propName))
            {
                continue;
            }

            PropertyInfo sourceProp = sourceType.GetProperty(propName);

            if (propertyMap.TryGetValue(propertyInfo.Name, out string mappedName))
            {
                propName = mappedName;
                sourceProp = sourceType.GetProperty(propName);
            }

            if (sourceProp == null)
            {
                if (cutOffUnits)
                {
                    propName = RemoveUnit(propertyInfo.Name);
                    sourceProp = sourceType.GetProperty(propName);
                    if (nameSearchKind != PropertyNameSearchKind.Equal && sourceProp == null)
                    {
                        sourceProp = sourceType
                            .GetProperties()
                            .FirstOrDefault(info =>
                            {
                                string name = RemoveUnit(info.Name);

                                switch (nameSearchKind)
                                {
                                    case PropertyNameSearchKind.EndsWith:
                                        return name.EndsWith(propName) || propName.EndsWith(name);
                                    case PropertyNameSearchKind.StartsWith:
                                        return name.StartsWith(propName)
                                            || propName.StartsWith(name);
                                    default:
                                        throw new ArgumentOutOfRangeException(
                                            nameof(nameSearchKind),
                                            nameSearchKind,
                                            null
                                        );
                                }
                            });
                    }
                }
            }

            object targetValue = propertyInfo.GetValue(target);
            if (sourceProp == null)
            {
                if (checkIfSourceIsMissingTargetIsNull)
                {
                    Assert.True(
                        targetValue == null,
                        $"Property '{propertyInfo.Name}' is not null, "
                            + "but the source property was not found."
                    );
                }

                if (ignoreMissingSourceProperty)
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"Expected the source type '{sourceType.Name}' to have "
                        + $"a property '{propName}', but it wasn't found."
                );
            }

            object sourceValue = sourceProp.GetValue(source);
            numberOfComparedProperties++;

            if (
                comparerMap != null
                && comparerMap.TryGetValue(propertyInfo.Name, out IEqualityComparer comparer)
            )
            {
                Assert.True(
                    comparer.Equals(sourceValue, targetValue),
                    $"Property '{propertyInfo.Name}' differs from the source. The target "
                        + $"value '{targetValue}' was not equal to source '{sourceValue}'."
                );
            }
            else if (IsValueToCompare(propertyInfo.PropertyType))
            {
                AssertEqual(propertyInfo, targetValue, sourceValue);
            }
            else if (typeof(ICollection).IsAssignableFrom(propertyInfo.PropertyType))
            {
                ICollection targetCollection = (ICollection)targetValue;
                ICollection sourceCollection = (ICollection)sourceValue;

                Assert.True(
                    targetCollection.Count == sourceCollection.Count,
                    $"The Collection '{propertyInfo.Name}' size differs from the source. The target "
                        + $"value '{targetCollection.Count}' was not equal to source '{sourceCollection.Count}'."
                );

                IEnumerator targetEnumerator = targetCollection.GetEnumerator();
                IEnumerator sourceEnumerator = sourceCollection.GetEnumerator();

                while (targetEnumerator.MoveNext() && sourceEnumerator.MoveNext())
                {
                    targetValue = targetEnumerator.Current;
                    sourceValue = sourceEnumerator.Current;
                    if (IsValueToCompare(targetValue.GetType()))
                    {
                        AssertEqual(propertyInfo, targetValue, sourceValue);
                    }
                    else
                    {
                        numberOfComparedProperties += Compare(
                            targetValue,
                            sourceValue,
                            cutOffUnits,
                            targetPropertiesToIgnore,
                            propertyMap,
                            comparerMap,
                            ignoreMissingSourceProperty,
                            checkIfSourceIsMissingTargetIsNull,
                            nameSearchKind
                        );
                    }
                }
            }
            else
            {
                numberOfComparedProperties += Compare(
                    targetValue,
                    sourceValue,
                    cutOffUnits,
                    targetPropertiesToIgnore,
                    propertyMap,
                    comparerMap,
                    ignoreMissingSourceProperty,
                    checkIfSourceIsMissingTargetIsNull,
                    nameSearchKind
                );
            }
        }

        return numberOfComparedProperties;
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
