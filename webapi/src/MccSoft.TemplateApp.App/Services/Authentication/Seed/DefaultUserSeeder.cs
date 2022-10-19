using System.Text.Json;
using MccSoft.TemplateApp.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MccSoft.TemplateApp.App.Services.Authentication;

public class DefaultUserSeeder
{
    private readonly IOptions<IdentityOptions> _identityOptions;
    private readonly UserManager<User> _userManager;

    public DefaultUserSeeder(
        IOptions<IdentityOptions> identityOptions,
        UserManager<User> userManager
    )
    {
        _identityOptions = identityOptions;
        _userManager = userManager;
    }

    public async Task SeedUser(string userName, string password)
    {
        User existingUser = await _userManager.FindByNameAsync(userName);
        if (existingUser != null)
        {
            var isPasswordCorrect = await _userManager.CheckPasswordAsync(existingUser, password);
            if (isPasswordCorrect)
                return;
        }

        // preserve password settings to later reset to them
        PasswordOptions passwordOptions = _identityOptions.Value.Password;
        var serializedPasswordOptions = JsonSerializer.Serialize(passwordOptions);
        try
        {
            // adjust Password settings to allow setting the default password
            passwordOptions.RequireNonAlphanumeric = false;
            passwordOptions.RequiredLength = 0;
            passwordOptions.RequireUppercase = false;
            passwordOptions.RequireLowercase = false;
            passwordOptions.RequireDigit = false;

            if (existingUser == null)
            {
                var user = new User() { UserName = userName, };
                await _userManager.CreateAsync(user);
                await _userManager.AddPasswordAsync(user, password);
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                var result = await _userManager.ResetPasswordAsync(existingUser, token, password);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Reset password failed, {result}");
                }
            }
        }
        finally
        {
            //reset settings to default
            _identityOptions.Value.Password = JsonSerializer.Deserialize<PasswordOptions>(
                serializedPasswordOptions
            );
        }
    }
}
