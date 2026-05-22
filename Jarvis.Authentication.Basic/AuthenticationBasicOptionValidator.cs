using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Basic;

public sealed class AuthenticationBasicOptionValidator : IValidateOptions<AuthenticationBasicOption>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationBasicOption options)
    {
        if (options.Users.Count == 0)
            return ValidateOptionsResult.Fail($"Authentication:Basic:{name ?? "scheme"}:Users must contain at least one user.");

        foreach (var (username, credential) in options.Users)
        {
            if (string.IsNullOrWhiteSpace(username))
                return ValidateOptionsResult.Fail("Authentication:Basic user name cannot be empty.");

            if (string.IsNullOrEmpty(credential.Password))
                return ValidateOptionsResult.Fail($"Authentication:Basic:Users:{username}:Password is required.");
        }

        return ValidateOptionsResult.Success;
    }
}
