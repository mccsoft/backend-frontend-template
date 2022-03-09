using MccSoft.TemplateApp.Domain;
using Microsoft.AspNetCore.Identity;

namespace MccSoft.TemplateApp.App.Controllers.Authentication;

public class AuthorizationController : AuthorizationControllerBase<User, string>
{
    public AuthorizationController(SignInManager<User> signInManager, UserManager<User> userManager)
        : base(signInManager, userManager) { }
}
