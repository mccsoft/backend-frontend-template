using MccSoft.WebApi.Domain.Helpers;

namespace MccSoft.TemplateApp.App.Features.Users.Dto;

public class CreatePasswordDto
{
    [AllowedPasswordChars]
    public string Password { get; set; }
}
