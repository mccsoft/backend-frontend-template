using Microsoft.AspNetCore.Builder;

namespace MccSoft.TemplateApp.App.Middleware
{
    public static class SignOutLockedUserMiddlewareExtensions
    {
        public static IApplicationBuilder UseSignOutLockedUser(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SignOutLockedUserMiddleware>();
        }
    }
}
