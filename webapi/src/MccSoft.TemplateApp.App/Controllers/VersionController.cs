using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MccSoft.TemplateApp.App.Controllers;

/// <summary>
/// Shows the info about the service.
/// </summary>
[ApiController]
public class VersionController
{
    public VersionController() { }

    /// <summary>
    /// Gets the version of the service.
    /// </summary>
    /// <returns>A string representing the version.</returns>
    [AllowAnonymous]
    [HttpGet("api")]
    public string Version()
    {
        var attribute = typeof(VersionController)
            .GetTypeInfo()
            .Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        if (attribute == null)
            throw new Exception("Can not retrieve api version");

        return attribute.InformationalVersion;
    }
}
