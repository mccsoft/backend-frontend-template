using MccSoft.TemplateApp.App.Features.Users.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MccSoft.TemplateApp.App.Features.Users;

[Authorize]
[Route("api/users")]
public class UserController
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Gets permissions for the current user
    /// </summary>
    [HttpGet("me")]
    public async Task<CurrentUserDto> GetCurrentUserInfo()
    {
        return await _userService.GetCurrentUserInfo();
    }

    /// <summary>
    /// Allows user to reset their password using single-use password reset token issued by the backend.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _userService.ResetPassword(dto);
    }

    /// <summary>
    /// Changes password by a user.
    /// </summary>
    /// <param name="dto">The dto contains old and new passwords.</param>
    [HttpPut("password")]
    public async Task ChangePassword(ChangePasswordDto dto)
    {
        await _userService.ChangePassword(dto);
    }
}
