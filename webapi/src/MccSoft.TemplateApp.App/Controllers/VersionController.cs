using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using MccSoft.Mailing;
using MccSoft.TemplateApp.App.Views.Emails.Example;
using MccSoft.WebApi.Domain.Helpers;
using MccSoft.WebApi.Patching.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MccSoft.TemplateApp.App.Controllers
{
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
            var attribute = typeof(VersionController).GetTypeInfo()
                .Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (attribute == null)
                throw new Exception("Can not retrieve api version");

            return attribute.InformationalVersion;
        }
    }
}
