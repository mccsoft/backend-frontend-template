using System;
using System.Threading.Tasks;
using MccSoft.LowLevelPrimitives;
using MccSoft.TemplateApp.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace MccSoft.TemplateApp.App.Middleware
{
    public class SignOutLockedUserMiddleware
    {
        private readonly RequestDelegate _next;

        public SignOutLockedUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext httpContext,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUserAccessor userAccessor
        )
        {
            if (userAccessor.IsUserAuthenticated)
            {
                var userId = userAccessor.GetUserId();
                var user = await userManager.FindByIdAsync(userId);
                if (user == null || user.LockoutEnabled && (user.LockoutEnd > DateTimeOffset.Now))
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(
                        JsonConvert.SerializeObject(
                            new
                            {
                                Message = user == null
                                    ? $"User with id '{userId}' wasn't found."
                                    : $"User '{user?.UserName}' is locked."
                            }
                        )
                    );
                    return;
                }
            }

            await _next(httpContext);
        }
    }
}
