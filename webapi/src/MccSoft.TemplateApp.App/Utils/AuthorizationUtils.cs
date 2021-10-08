using System;
using System.Security.Claims;
using System.Security.Principal;
using IdentityModel;
using IdentityServer4.Extensions;
using MccSoft.TemplateApp.App.Services.Authentication;

namespace MccSoft.TemplateApp.App.Utils
{
    public static class AuthorizationUtils
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        public static string GetUserId(this IPrincipal principal)
        {
            return (principal.Identity ?? throw new InvalidOperationException()).GetUserId();
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">id claim is missing</exception>
        public static string GetUserId(this IIdentity identity)
        {
            var claimValue = identity.GetUserIdOrNull();

            if (claimValue == null)
                throw new InvalidOperationException("id claim is missing");
            return claimValue;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">id claim is missing</exception>
        public static string? GetUserIdOrNull(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(JwtClaimTypes.Id);

            return claim?.Value;
        }
    }
}
