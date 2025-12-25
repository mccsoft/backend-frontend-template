using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;

namespace MccSoft.WebApi.Domain.Helpers;

public class AllowedPasswordCharsAttribute : ValidationAttribute
{
    private const string Pattern = @"^[A-Za-z0-9 !""#$%&'()*+,\-./:;<=>?@\[\]\\\^_{|}~]+$";

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var str = value as string;
        if (string.IsNullOrEmpty(str))
            return ValidationResult.Success;

        if (str.Trim() != str)
            return new ValidationResult("Spaces not allowed at start or end");

        if (!Regex.IsMatch(str, Pattern))
            return new ValidationResult(
                "Allowed password characters are: A-Z, a-z, 0-9 and !@#$%^&*.()"
            );

        return ValidationResult.Success;
    }
}
