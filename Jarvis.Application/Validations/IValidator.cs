namespace Jarvis.Application.Validations;

public interface IValidator
{

}

public interface IValidator<T> : IValidator
{
    Task<ValidationResult> HandleAsync(T input);
}