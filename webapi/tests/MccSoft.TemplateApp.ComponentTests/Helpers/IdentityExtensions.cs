using System.Linq;
using System.Security.Claims;
using IdentityModel;

namespace MccSoft.TemplateApp.ComponentTests.Helpers;

public static class IdentityExtensions
{
    public static ClaimsIdentity SetUserIdClaim(this ClaimsIdentity identity, string value)
    {
        return identity.SetClaim(JwtClaimTypes.Id, value);
    }

    private static ClaimsIdentity SetClaim(this ClaimsIdentity identity, string type, string value)
    {
        var existingClaim = identity.Claims.FirstOrDefault(x => x.Type == type);
        if (existingClaim != null)
        {
            identity.RemoveClaim(existingClaim);
        }

        identity.AddClaim(new Claim(type, value));

        return identity;
    }
}
