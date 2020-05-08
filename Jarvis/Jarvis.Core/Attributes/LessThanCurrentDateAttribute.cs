using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Jarvis.Core.Attributes
{
    public class LessThanCurrentDateAttribute : ValidationAttribute
    {
        public LessThanCurrentDateAttribute()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var datetime = (DateTime)value;
            if (datetime.Date > DateTime.Now.Date)
                return new ValidationResult(ErrorMessage);
            return ValidationResult.Success;
        }
    }
}
