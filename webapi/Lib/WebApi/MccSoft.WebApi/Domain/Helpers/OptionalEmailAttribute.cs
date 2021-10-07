using System.ComponentModel.DataAnnotations;

namespace MccSoft.WebApi.Domain.Helpers
{
    public class OptionalEmailAddressAttribute : ValidationAttribute
    {
        private EmailAddressAttribute _emailAddressAttribute;

        public OptionalEmailAddressAttribute()
        {
            _emailAddressAttribute = new EmailAddressAttribute();
        }

        public override bool IsValid(object value)
        {
            if (value != null && value.Equals(string.Empty))
                return true;

            return _emailAddressAttribute.IsValid(value);
        }
    }
}
