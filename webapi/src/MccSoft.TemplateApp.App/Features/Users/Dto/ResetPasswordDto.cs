namespace MccSoft.TemplateApp.App.Features.Users.Dto;

public class ResetPasswordDto
{
    public string Username { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
