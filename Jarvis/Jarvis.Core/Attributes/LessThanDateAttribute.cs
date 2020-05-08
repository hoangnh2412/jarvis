using System;
using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Attributes
{
    public class LessThanDateAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public LessThanDateAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var property = validationContext.ObjectType.GetProperty(_otherProperty);
            if (property == null)
                throw new ArgumentException($"Property {_otherProperty} not found");

            var otherValue = property.GetValue(validationContext.ObjectInstance);
            if (otherValue == null)
                return ValidationResult.Success;

            var comparisonValue = (DateTime)property.GetValue(validationContext.ObjectInstance);
            var currentValue = (DateTime)value;

            if (currentValue > comparisonValue)
                return new ValidationResult(ErrorMessage);
            return ValidationResult.Success;
        }
    }
}
