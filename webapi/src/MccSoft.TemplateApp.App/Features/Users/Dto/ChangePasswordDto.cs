using System.ComponentModel.DataAnnotations;

namespace MccSoft.TemplateApp.App.Features.Users.Dto;

public class ChangePasswordDto
{
    [Required]
    public string OldPassword { get; set; }

    [Required]
    public string NewPassword { get; set; }
}
