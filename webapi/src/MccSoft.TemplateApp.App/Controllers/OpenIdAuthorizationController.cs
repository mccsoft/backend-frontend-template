using MccSoft.TemplateApp.Domain;
using Microsoft.AspNetCore.Identity;
using Shaddix.OpenIddict.ExternalAuthentication;
using Shaddix.OpenIddict.ExternalAuthentication.Infrastructure;

namespace MccSoft.TemplateApp.App.Controllers;

public class OpenIdAuthorizationController : OpenIdAuthorizationControllerBase<User, string>
{
    public OpenIdAuthorizationController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IOpenIddictClientConfigurationProvider configurationProvider,
        ILogger<OpenIdAuthorizationController> logger
    ) : base(signInManager, userManager, configurationProvider, logger) { }
}
