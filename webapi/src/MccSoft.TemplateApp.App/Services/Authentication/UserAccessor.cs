using System.Security.Principal;
using System.Threading;
using IdentityModel;
using MccSoft.LowLevelPrimitives;
using MccSoft.LowLevelPrimitives.Exceptions;
using Microsoft.AspNetCore.Http;

namespace MccSoft.TemplateApp.App.Services.Authentication
{
    public class UserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public string GetUserId()
        {
            var user = GetUserIdentity();

            return user.GetClaimValue(JwtClaimTypes.Id);
        }

        /// <inheritdoc/>
        public bool IsHttpContextAvailable => _httpContextAccessor.HttpContext != null;

        /// <inheritdoc/>
        public bool IsUserAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true
            && !IsAuthenticationInProgress;

        private bool IsAuthenticationInProgress =>
            IsHttpContextAvailable
            && _httpContextAccessor.HttpContext.Request.Path.StartsWithSegments("/connect/token");

        private IIdentity GetUserIdentity()
        {
            var identity = _httpContextAccessor.HttpContext?.User?.Identity;
            if (identity == null)
                throw new AccessDeniedException("User is not authenticated");

            return identity;
        }
    }
}
