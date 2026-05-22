namespace Jarvis.Authentication;

public interface IPasswordPolicyValidator
{
    Task<PasswordValidationResult> ValidateAsync(string password, CancellationToken cancellationToken = default);
}
