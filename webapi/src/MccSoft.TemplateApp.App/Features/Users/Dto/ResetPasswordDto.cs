using MccSoft.WebApi.Domain.Helpers;

namespace MccSoft.TemplateApp.App.Features.Users.Dto;

public class ResetPasswordDto
{
    public string Username { get; set; }
    public string Token { get; set; }

    [AllowedPasswordChars]
    public string NewPassword { get; set; }
}
