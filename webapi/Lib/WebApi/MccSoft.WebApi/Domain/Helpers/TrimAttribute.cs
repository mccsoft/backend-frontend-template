using System.ComponentModel.DataAnnotations;

namespace MccSoft.WebApi.Domain.Helpers;

/// <summary>
/// Trims the string that is passed from the client
/// </summary>
public class TrimAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string stringValue)
            return ValidationResult.Success;
        if (string.IsNullOrEmpty(stringValue))
            return ValidationResult.Success;
        var trimmedValue = stringValue.Trim();
        if (trimmedValue == stringValue)
            return ValidationResult.Success;

        var parent = validationContext.ObjectInstance;
        var type = parent.GetType();
        type.GetProperty(validationContext.MemberName!)!.SetValue(parent, trimmedValue);
        return ValidationResult.Success;
    }
}
