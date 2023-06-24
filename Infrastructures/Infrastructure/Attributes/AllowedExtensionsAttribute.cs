using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Attributes
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            if (file == null)
                return ValidationResult.Success;

            var extension = Path.GetExtension(file.FileName);
            if (_extensions.Contains(extension.ToLower()))
                return ValidationResult.Success;

            return new ValidationResult($"This extension {extension} is not allowed!");
        }
    }
}