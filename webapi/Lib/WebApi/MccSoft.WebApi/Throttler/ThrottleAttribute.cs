﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityModel;
using MccSoft.LowLevelPrimitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MccSoft.WebApi.Throttler;

/// <summary>
/// Attribute that configures throttling for API requests
/// </summary>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true
)]
public class ThrottleAttribute : ActionFilterAttribute
{
    private Throttler _throttler;
    private readonly string _throttlingConfigSectionName;
    private readonly string _throttleGroup;

    /// <param name="throttlingConfigSectionName">
    /// Path to section in appsettings.json which configures <see cref="ThrottleConfigOptions.RequestLimit"/> and <see cref="ThrottleConfigOptions.TimeoutInSeconds"/>.
    /// E.g.
    /// "IsValidDevice":
    /// {
    ///    "RequestLimit": 200,
    ///    "TimeoutInSeconds": 3600
    /// }
    /// allows not more than 200 requests per hour.
    /// </param>
    /// <param name="throttleGroup">
    /// Use one of <classref name="ThrottleGroup"/> constants if you want to limit
    /// API per user/IP.
    /// </param>
    public ThrottleAttribute(string throttlingConfigSectionName, string throttleGroup = "")
    {
        _throttlingConfigSectionName = throttlingConfigSectionName;
        _throttleGroup = throttleGroup;
    }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        InitializeThrottlerIfNotYet(context.HttpContext);

        var httpContext = context.HttpContext;
        SetIdentityAsThrottleGroup(context);

        if (_throttler.ShouldRequestBeThrottled(out Throttler.ThrottleInfo throttleInfo))
        {
            // Unfortunately, there's no HttpStatusCode.TooManyRequests code in netstandard2.0 yet.
            // After update to netstandard2.1, we can remove this constant.
            const int httpStatusCodeTooManyRequests = 429;

            var details = new ProblemDetails
            {
                Type = ErrorTypes.TooManyRequests,
                Title = "Too many requests",
                Detail =
                    $"Number of requests: {throttleInfo.RequestCount}, "
                    + $"Expires: {throttleInfo.ExpiresAt:s}. Group: {_throttler.ThrottleGroup}."
            };

            var result = new ObjectResult(details) { StatusCode = httpStatusCodeTooManyRequests };

            AddThrottleHeaders(httpContext.Response);
            await ExecuteResult(httpContext, result);
            LogTooManyRequests(context.HttpContext);
            return;
        }

        _throttler.IncrementRequestCount();

        await next();

        AddThrottleHeaders(httpContext.Response);
    }

    private static async Task ExecuteResult(HttpContext httpContext, IActionResult result)
    {
        RouteData routeData = httpContext.GetRouteData() ?? new RouteData();
        var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
        await result.ExecuteResultAsync(actionContext);
    }

    private void InitializeThrottlerIfNotYet(HttpContext httpContext)
    {
        if (_throttler != null)
        {
            return;
        }

        // Throttling settings are stored in appsettings to allow overriding them in Tests
        var globalConfiguration = httpContext.RequestServices.GetService<IConfiguration>();
        IConfigurationSection configSection = globalConfiguration.GetSection(
            _throttlingConfigSectionName
        );
        var throttlerConfiguration = configSection.Get<ThrottleConfigOptions>();

        _throttler = new Throttler(
            _throttleGroup,
            throttlerConfiguration.RequestLimit,
            TimeSpan.FromSeconds(throttlerConfiguration.TimeoutInSeconds)
        );
    }

    private void LogTooManyRequests(HttpContext httpContext)
    {
        var logger = httpContext.RequestServices.GetService<ILogger<ThrottleAttribute>>();
        logger.LogWarning(
            $"Too many requests to group '{_throttleGroup}', URL: {httpContext.Request.GetDisplayUrl()}"
        );
    }

    private void SetIdentityAsThrottleGroup(ActionExecutingContext context)
    {
        if (_throttleGroup == ThrottleGroup.Identity)
        {
            _throttler.ThrottleGroup = context.HttpContext.User
                .FindFirst(JwtClaimTypes.Subject)
                .Value;
        }

        if (_throttleGroup == ThrottleGroup.IpAddress)
        {
            _throttler.ThrottleGroup = context.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }

    private void AddThrottleHeaders(HttpResponse response)
    {
        if (response == null)
        {
            return;
        }

        foreach (KeyValuePair<string, string> header in _throttler.GetRateLimitHeaders())
        {
            response.Headers[header.Key] = header.Value;
        }
    }
}
