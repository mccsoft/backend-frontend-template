using System.ComponentModel.DataAnnotations;
using MccSoft.WebApi.Domain.Helpers;

namespace MccSoft.TemplateApp.App.Features.Users.Dto;

public class ChangePasswordDto
{
    [Required]
    public string OldPassword { get; set; }

    [Required]
    [AllowedPasswordChars]
    public string NewPassword { get; set; }
}
