using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Jarvis.Core.Attributes
{
    public class FileRecordAttribute : ValidationAttribute
    {
        private readonly int _otherProperty;

        public FileRecordAttribute(int otherProperty)
        {
            _otherProperty = otherProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var base64 = (string)value;

            if (!string.IsNullOrEmpty(base64))
            {
                var bytes = Convert.FromBase64String(base64);
                var length = bytes.Length;

                var mbSize = _otherProperty * 1024; //giới hạn dung lượng của file tính theo KB
                if (length > mbSize)
                    return new ValidationResult(ErrorMessage = $"Dung lượng tối đa {_otherProperty}MB");
            }

            return ValidationResult.Success;
        }
    }
}
