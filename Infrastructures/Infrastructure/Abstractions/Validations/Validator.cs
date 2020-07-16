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

            foreach (var validator in _validators)
            {
                var type = validator.GetType();

                var rule = type.GetInterfaces().FirstOrDefault(x => x.FullName == typeof(IValidationRuleAsync<T, ValidationResult>).FullName);
                if (rule != null)
                {
                    result = await ((IValidationRuleAsync<T, ValidationResult>)validator).HandleAsync(input);
                    if (!result.Success)
                        return result;
                }

                rule = type.GetInterfaces().FirstOrDefault(x => x.FullName == typeof(IValidationRule<T, ValidationResult>).FullName);
                if (rule != null)
                {
                    result = ((IValidationRule<T, ValidationResult>)validator).Handle(input);
                    if (!result.Success)
                        return result;
                }
            }

            return new ValidationResult(true);
        }

        private ValidationResult ModelValidate(object input)
        {
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(input, null, null);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(input, context, validationResults, true);
            var message = string.Join("; ", validationResults.Select(x => x.ErrorMessage));
            return new ValidationResult(isValid, message);
        }
    }
}