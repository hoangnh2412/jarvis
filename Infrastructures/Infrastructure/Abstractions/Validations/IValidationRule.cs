using System.Threading.Tasks;

namespace Infrastructure.Abstractions.Validations
{
    public interface IValidationRule
    {
        
    }

    public interface IValidationRuleAsync<TInput, TOutput> : IValidationRule
    {
        Task<TOutput> HandleAsync(TInput input);
    }

    public interface IValidationRule<TInput, TOutput> : IValidationRule
    {
        TOutput Handle(TInput input);
    }
}