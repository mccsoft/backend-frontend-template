using System.Security.Claims;
using MccSoft.LowLevelPrimitives;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Shaddix.OpenIddict.ExternalAuthentication;
using Shaddix.OpenIddict.ExternalAuthentication.Infrastructure;

namespace MccSoft.TemplateApp.App.Controllers;

[Log]
public class OpenIdAuthorizationController : OpenIdAuthorizationControllerBase<User, string>
{
    private readonly TemplateAppDbContext _dbContext;
    private readonly ILogger<OpenIdAuthorizationController> _logger;

    public OpenIdAuthorizationController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IOpenIddictClientConfigurationProvider configurationProvider,
        TemplateAppDbContext dbContext,
        ILogger<OpenIdAuthorizationController> logger
    ) : base(signInManager, userManager, configurationProvider, logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    protected override async Task<IList<Claim>> GetClaims(
        User user,
        OpenIddictRequest openIddictRequest
    )
    {
        var result = await base.GetClaims(user, openIddictRequest);
        result.Add(new Claim(CustomTenantIdAccessor.TenantIdClaim, user.TenantId.ToString()));
        return result;
    }

    public override Task<IActionResult> Authorize(string? provider)
    {
        using var ignoreFilter = CustomTenantIdAccessor.IgnoreTenantIdQueryFilter();
        return base.Authorize(provider);
    }

    public override Task<IActionResult> Exchange()
    {
        using var ignoreFilter = CustomTenantIdAccessor.IgnoreTenantIdQueryFilter();
        return base.Exchange();
    }

    protected override async Task<User?> CreateNewUser(ExternalLoginInfo externalUserInfo)
    {
        var tenant = new Tenant();
        _dbContext.Add(tenant);
        await _dbContext.SaveChangesAsync();

        var user = await base.CreateNewUser(externalUserInfo);
        user!.SetTenantIdUnsafe(tenant.Id);
        return user;
    }
}
