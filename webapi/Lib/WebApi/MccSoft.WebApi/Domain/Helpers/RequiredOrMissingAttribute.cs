using System.ComponentModel.DataAnnotations;
using MccSoft.WebApi.Patching.Models;

namespace MccSoft.WebApi.Domain.Helpers
{
    /// <summary>
    /// Shall be applied to properties of IPatchRequest-descendant models.
    /// Attribute prevents from passing `null` values to a properties, but allows `undefined` values (property is missing in json).
    /// If the DTO is not an IPatchRequest this attribute acts just like a regular <see cref="RequiredAttribute"/>.
    /// </summary>
    public class RequiredOrMissingAttribute : RequiredAttribute
    {
        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            if (validationContext.ObjectInstance is IPatchRequest patchRequest)
            {
                if (!patchRequest.IsFieldPresent(validationContext.MemberName))
                {
                    return ValidationResult.Success;
                }
            }

            return base.IsValid(value, validationContext);
        }
    }
}
