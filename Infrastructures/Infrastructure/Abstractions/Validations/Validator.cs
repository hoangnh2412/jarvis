using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Abstractions.Validations
{
    public class Validator<T> : IValidator<T>
    {
        private readonly IServiceProvider _provider;
        protected readonly IEnumerable<IValidationRule> _validators;

        public Validator(IServiceProvider provider, IEnumerable<Type> types)
        {
            _provider = provider;

            var validators = new List<IValidationRule>();
            var serviceValidators = provider.GetServices<IValidationRule>();
            foreach (var type in types)
            {
                var validator = serviceValidators.FirstOrDefault(x => x.GetType().FullName == type.FullName);
                if (validator != null)
                    validators.Add(validator);
            }
            _validators = validators;
        }

        public async Task<ValidationResult> HandleAsync(T input)
        {
            if (input == null)
                return new ValidationResult(false);

            var result = ModelValidate(input);
            if (!result.Success)
                return result;

            foreach (IValidationRule<T, ValidationResult> validator in _validators)
            {
                result = await validator.HandleAsync(input);
                if (!result.Success)
                    return result;
            }

            return new ValidationResult(true);
        }

        private ValidationResult ModelValidate(object input)
        {
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(input);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(input, context, results);
            var message = string.Join(';', results.Select(x => x.ErrorMessage));
            return new ValidationResult(isValid, message);
        }
    }
}