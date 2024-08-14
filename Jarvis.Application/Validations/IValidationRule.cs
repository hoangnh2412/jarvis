namespace Jarvis.Application.Validations;

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