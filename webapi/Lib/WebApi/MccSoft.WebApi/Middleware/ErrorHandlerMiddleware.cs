using System;
using System.Threading.Tasks;
using MccSoft.LowLevelPrimitives;
using MccSoft.LowLevelPrimitives.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MccSoft.WebApi.Middleware
{
    /// <summary>
    ///     Converts each exception that occurs in the application to an appropriate HTTP response.
    /// </summary>
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _env;
        private static readonly RouteData _emptyRouteData = new RouteData();
        private static readonly ActionDescriptor _emptyAction = new ActionDescriptor();

        public ErrorHandlerMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlerMiddleware> logger,
            IWebHostEnvironment env
        ) {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex) when (ex is IWebApiException wae)
            {
                await ProcessWebApiException(httpContext, ex, wae.Result);
            }
            catch (Exception ex)
            {
                await ProcessException(httpContext, ex);
            }
        }

        private static async Task ExecuteResult(HttpContext httpContext, IActionResult result)
        {
            RouteData routeData = httpContext.GetRouteData() ?? _emptyRouteData;
            var actionContext = new ActionContext(httpContext, routeData, _emptyAction);
            await result.ExecuteResultAsync(actionContext);
        }

        private async Task ProcessException(HttpContext httpContext, Exception ex)
        {
            _logger.LogError($"An unhandled exception has occurred: {ex}");
            string detail = _env.IsProduction() ? null : ex.ToString();
            var details = new ProblemDetails
            {
                Type = ErrorTypes.InternalServerError,
                Title = "A server error has occurred.",
                Detail = detail
            };

            var result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };

            await ExecuteResult(httpContext, result);
        }

        private async Task ProcessWebApiException(
            HttpContext httpContext,
            Exception ex,
            IActionResult result
        ) {
            _logger.LogWarning($"{ex.GetType().Name}: {ex.Message}");
            await ExecuteResult(httpContext, result);
        }
    }
}
