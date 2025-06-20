namespace MccSoft.TemplateApp.App.Features.Users.Dto;

public class CurrentUserDto
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Nickname { get; set; }
    public List<string> Permissions { get; set; }
}
