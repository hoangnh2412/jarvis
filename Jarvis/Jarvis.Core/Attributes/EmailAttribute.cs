using System.ComponentModel.DataAnnotations;
using Infrastructure.Extensions;

namespace Jarvis.Core.Attributes
{
    public class EmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var source = (string)value;

            if (!string.IsNullOrEmpty(source) && !source.IsEmails(';'))
                return new ValidationResult(ErrorMessage = $"{source} định dạng email không đúng");

            return ValidationResult.Success;
        }

    }
}