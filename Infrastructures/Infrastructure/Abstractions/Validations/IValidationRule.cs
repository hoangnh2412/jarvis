using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Abstractions.Validations
{
    public interface IValidationRule
    {
        
    }

    public interface IValidationRule<TInput, TOutput> : IValidationRule
    {
        Task<TOutput> HandleAsync(TInput input);
    }
}