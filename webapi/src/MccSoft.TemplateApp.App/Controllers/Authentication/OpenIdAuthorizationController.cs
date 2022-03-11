using MccSoft.TemplateApp.Domain;
using MccSoft.WebApi.Authentication;
using Microsoft.AspNetCore.Identity;

namespace MccSoft.TemplateApp.App.Controllers.Authentication;

public class OpenIdAuthorizationController : OpenIdAuthorizationControllerBase<User, string>
{
    public OpenIdAuthorizationController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IOpenIddictConfigurationProvider configurationProvider
    ) : base(signInManager, userManager, configurationProvider) { }
}
