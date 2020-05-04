using System.Threading.Tasks;

namespace Infrastructure.Abstractions.Validations
{
    public interface IValidator
    {
        
    }

    public interface IValidator<T> : IValidator
    {
        Task<ValidationResult> HandleAsync(T input);
    }
}