using MccSoft.Logging;
using MccSoft.LowLevelPrimitives;
using MccSoft.TemplateApp.Domain;
using MccSoft.WebApi.Serialization;
using Microsoft.AspNetCore.Identity;

namespace MccSoft.TemplateApp.App.Middleware;

public class SignOutLockedUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SignOutLockedUserMiddleware> _logger;

    public SignOutLockedUserMiddleware(
        RequestDelegate next,
        ILogger<SignOutLockedUserMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
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
                if (user == null)
                {
                    _logger.LogInformation(
                        $"User with id=${Field.UserId} wasn't found in the database, logging out",
                        userId
                    );
                }
                else
                {
                    _logger.LogInformation(
                        $"User with id=${Field.UserId} was locked. Logging out",
                        userId
                    );
                }

                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(
                    DefaultJsonSerializer.Serialize(
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
