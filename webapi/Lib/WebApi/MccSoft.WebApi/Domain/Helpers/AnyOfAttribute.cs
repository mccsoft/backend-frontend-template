using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MccSoft.WebApi.Domain.Helpers;

public class AnyOfThoseAttribute : ValidationAttribute
{
    public object[] PossibleValues { get; set; }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (PossibleValues.Any(x => x.Equals(value)))
            return ValidationResult.Success;
        else
            return new ValidationResult(FormatErrorMessage(validationContext.MemberName));
    }
}
