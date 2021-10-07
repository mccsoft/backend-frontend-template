using MccSoft.WebApi.Middleware;
using Microsoft.AspNetCore.Builder;

namespace MccSoft.WebApi
{
    public static class WebApiRegistrationExtensions
    {
        /// <summary>
        /// Adds a middleware to handle exceptions, including custom ones.
        /// </summary>
        /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
