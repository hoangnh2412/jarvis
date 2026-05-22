using Microsoft.Extensions.Options;

namespace Jarvis.Authentication;

public sealed class AuthenticationRootOptionsValidator : IValidateOptions<AuthenticationRootOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationRootOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Type))
            return ValidateOptionsResult.Fail("Authentication:Type is required.");

        return ValidateOptionsResult.Success;
    }
}
