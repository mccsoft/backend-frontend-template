using System;
using System.Threading.Tasks;
using MccSoft.LowLevelPrimitives;
using MccSoft.LowLevelPrimitives.Exceptions;
using MccSoft.WebApi.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Diagnostics;
using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;

namespace MccSoft.WebApi.Middleware
{
    /// <summary>
    ///     Converts each exception that occurs in the application to an appropriate HTTP response.
    /// </summary>
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IHostEnvironment _env;
        private static readonly RouteData _emptyRouteData = new RouteData();
        private static readonly ActionDescriptor _emptyAction = new ActionDescriptor();

        public ErrorHandlerMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlerMiddleware> logger,
            IHostEnvironment env
        )
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                httpContext.Request.EnableBuffering();
                await _next(httpContext);
            }
            catch (Exception ex) when (ex is IWebApiException wae)
            {
                await ProcessWebApiException(httpContext, ex, wae.Result);
            }
            catch (Exception ex) when (ex is BadHttpRequestException)
            {
                // Pass the system exception through, otherwise we are breaking this part of ASP.Net:
                // https://github.com/dotnet/aspnetcore/blob/b463e049b6aed02f94edcac8855b8b5c87d0989b/src/Servers/Kestrel/Core/src/Internal/Http2/Http2Connection.cs#L1200
                throw;
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
            string body = await httpContext.Request.ReadAll();
            LogContext.PushProperty("Request body", body);

            // TODO: Replace `{ex}` in the message with just the type and message when we upgrade Elasticsearch to v7.
            // Until then we put the call stack into the message to make it searchable in Kibana.
            // See https://github.com/elastic/kibana/issues/1084#issuecomment-585178079
            _logger.LogError(ex, $"An unhandled exception has occurred: {ex}");
            string detail = _env.IsProduction() ? null : ex.ToString();
            var details = new ProblemDetails
            {
                Type = ErrorTypes.InternalServerError,
                Title = "A server error has occurred.",
                Detail = detail
            };
            AddTraceId(httpContext, details);
            var result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };

            await ExecuteResult(httpContext, result);
        }

        private static void AddTraceId(HttpContext httpContext, ProblemDetails details)
        {
            string traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
            if (traceId is { })
            {
                details.Extensions["traceId"] = traceId;
            }
        }

        private async Task ProcessWebApiException(
            HttpContext httpContext,
            Exception ex,
            IActionResult result
        )
        {
            _logger.LogWarning("{ErrorType}: {ErrorMessage}", ex.GetType().Name, ex.Message);
            if (result is ObjectResult { Value: ProblemDetails details })
            {
                AddTraceId(httpContext, details);
            }

            await ExecuteResult(httpContext, result);
        }
    }
}
