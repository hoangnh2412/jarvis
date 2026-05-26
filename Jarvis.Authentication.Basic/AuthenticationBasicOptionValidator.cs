using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Basic;

/// <summary>
/// Validate <see cref="AuthenticationBasicOption"/> lúc startup.
/// </summary>
public sealed class AuthenticationBasicOptionValidator(bool requireUsers = true) : IValidateOptions<AuthenticationBasicOption>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationBasicOption options)
    {
        if (requireUsers && options.Users.Count == 0)
            return ValidateOptionsResult.Fail($"Authentication:Basic:{name ?? "scheme"}:Users must contain at least one user.");

        if (requireUsers)
        {
            foreach (var (username, credential) in options.Users)
            {
                if (string.IsNullOrWhiteSpace(username))
                    return ValidateOptionsResult.Fail("Authentication:Basic user name cannot be empty.");

                if (string.IsNullOrEmpty(credential.Password))
                    return ValidateOptionsResult.Fail($"Authentication:Basic:Users:{username}:Password is required.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}
