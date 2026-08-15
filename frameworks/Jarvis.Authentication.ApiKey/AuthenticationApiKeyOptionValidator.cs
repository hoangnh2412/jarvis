using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.ApiKey;

/// <summary>
/// Validate <see cref="AuthenticationApiKeyOption"/> lúc startup — đảm bảo <c>KeyName</c> và <c>Key</c> hợp lệ.
/// </summary>
public sealed class AuthenticationApiKeyOptionValidator : IValidateOptions<AuthenticationApiKeyOption>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationApiKeyOption options)
    {
        if (string.IsNullOrWhiteSpace(options.KeyName))
            return ValidateOptionsResult.Fail($"Authentication:ApiKey:{name ?? "realm"}:KeyName is required.");

        if (string.IsNullOrWhiteSpace(options.Key))
            return ValidateOptionsResult.Fail($"Authentication:ApiKey:{name ?? "realm"}:Key is required.");

        return ValidateOptionsResult.Success;
    }
}
