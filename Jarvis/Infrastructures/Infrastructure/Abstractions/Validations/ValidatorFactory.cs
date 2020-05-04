using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Abstractions.Validations
{
    public interface IValidatorFactory
    {
        IValidator<T> GetValidator<T>();
    }

    public class ValidatorFactory : IValidatorFactory
    {
        private readonly IEnumerable<IValidator> _validators;

        public ValidatorFactory(IEnumerable<IValidator> validators) {
            _validators = validators;
        }

        public IValidator<T> GetValidator<T>()
        {
            return _validators.FirstOrDefault(x => x.GetType().FullName == typeof(Validator<T>).FullName) as IValidator<T>;
        }
    }
}