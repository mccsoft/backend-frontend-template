using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace MccSoft.HttpClientExtension
{
    public static class ClaimExtensions
    {
        public static string? GetClaimValueOrNull(this IIdentity identity, string claimType)
        {
            if (identity is ClaimsIdentity id)
            {
                var claim = id.FindFirst(claimType);

                if (claim == null)
                {
                    return null;
                }

                return claim.Value;
            }

            throw new InvalidOperationException($"Identity {identity} is not ClaimsIdentity");
        }

        public static string GetClaimValue(this IIdentity identity, string claimType)
        {
            var result = GetClaimValueOrNull(identity, claimType);
            if (result == null)
            {
                throw new InvalidOperationException($"Claim '{claimType}' is missing");
            }

            return result;
        }

        public static IEnumerable<Claim> GetAllClaims(this IIdentity identity, string claimType)
        {
            if (identity is ClaimsIdentity id)
            {
                return id.FindAll(claimType);
            }

            throw new InvalidOperationException($"Identity {identity} is not ClaimsIdentity");
        }
    }
}