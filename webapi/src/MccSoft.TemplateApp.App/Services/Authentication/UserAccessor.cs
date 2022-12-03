using System.Security.Principal;
using System.Threading;
using IdentityModel;
using MccSoft.HttpClientExtension;
using MccSoft.LowLevelPrimitives;
using MccSoft.LowLevelPrimitives.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MccSoft.TemplateApp.App.Services.Authentication;

public class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<IdentityOptions> _identityOptions;

    public UserAccessor(
        IHttpContextAccessor httpContextAccessor,
        IOptions<IdentityOptions> identityOptions
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _identityOptions = identityOptions;
    }

    /// <inheritdoc/>
    public string GetUserId()
    {
        var user = GetUserIdentity();

        return user.GetClaimValue(_identityOptions.Value.ClaimsIdentity.UserIdClaimType);
    }

    /// <inheritdoc/>
    public bool IsHttpContextAvailable => _httpContextAccessor.HttpContext != null;

    /// <inheritdoc/>
    public bool IsUserAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    private IIdentity GetUserIdentity()
    {
        var identity = _httpContextAccessor.HttpContext?.User?.Identity;
        if (identity == null)
            throw new AccessDeniedException("User is not authenticated");

        return identity;
    }

    public int GetTenantId()
    {
        var customTenantId = ((IUserAccessor)this).GetCustomTenantId();
        if (customTenantId != null)
        {
            return customTenantId.Value;
        }

        return int.Parse(GetUserIdentity().GetClaimValue(CustomTenantIdAccessor.TenantIdClaim));
    }
}
