using System.Threading.Tasks;
using MccSoft.LowLevelPrimitives.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MccSoft.TemplateApp.App.Middleware
{
    /// <summary>
    ///     Converts exception from Persistence to WebApi Exceptions that are handled by ErrorHandlerMiddleware
    /// </summary>
    public class RethrowErrorsFromPersistenceMiddleware
    {
        private RequestDelegate _next;
        private readonly ILogger<RethrowErrorsFromPersistenceMiddleware> _logger;

        public RethrowErrorsFromPersistenceMiddleware(
            RequestDelegate next,
            ILogger<RethrowErrorsFromPersistenceMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (PersistenceAccessDeniedException ex)
            {
                _logger.LogWarning($"{ex.GetType().Name}: {ex.Message}");
                throw new AccessDeniedException(ex.Message);
            }
        }
    }
}
