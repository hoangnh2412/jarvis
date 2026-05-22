using Microsoft.Extensions.Options;

namespace Jarvis.Authentication;

public sealed class DefaultPasswordPolicyValidator(IOptions<AuthenticationRootOptions> rootOptions) : IPasswordPolicyValidator
{
    public Task<PasswordValidationResult> ValidateAsync(string password, CancellationToken cancellationToken = default)
    {
        var policy = rootOptions.Value.PasswordPolicy;
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password) || password.Length < policy.MinLength)
            errors.Add($"Password must be at least {policy.MinLength} characters.");

        if (policy.RequireDigit && !password.Any(char.IsDigit))
            errors.Add("Password must contain a digit.");

        if (policy.RequireUppercase && !password.Any(char.IsUpper))
            errors.Add("Password must contain an uppercase letter.");

        if (policy.RequireLowercase && !password.Any(char.IsLower))
            errors.Add("Password must contain a lowercase letter.");

        if (policy.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
            errors.Add("Password must contain a non-alphanumeric character.");

        return Task.FromResult(errors.Count == 0
            ? PasswordValidationResult.Success()
            : PasswordValidationResult.Failed(errors.ToArray()));
    }
}
