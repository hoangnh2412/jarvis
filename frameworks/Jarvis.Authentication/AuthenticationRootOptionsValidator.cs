using Microsoft.Extensions.Options;

namespace Jarvis.Authentication;

/// <summary>
/// Validate <see cref="AuthenticationRootOptions"/> lúc startup — yêu cầu <see cref="AuthenticationRootOptions.Type"/> không rỗng.
/// </summary>
public sealed class AuthenticationRootOptionsValidator : IValidateOptions<AuthenticationRootOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationRootOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Type))
            return ValidateOptionsResult.Fail("Authentication:Type is required.");

        return ValidateOptionsResult.Success;
    }
}
