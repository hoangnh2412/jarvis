using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.ApiKey;

public sealed class AuthenticationApiKeyOptionValidator : IValidateOptions<AuthenticationApiKeyOption>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationApiKeyOption options)
    {
        if (string.IsNullOrWhiteSpace(options.KeyName))
            return ValidateOptionsResult.Fail($"Authentication:ApiKey:{name ?? "scheme"}:KeyName is required.");

        if (options.Keys.Length == 0)
            return ValidateOptionsResult.Fail($"Authentication:ApiKey:{name ?? "scheme"}:Keys must contain at least one key.");

        return ValidateOptionsResult.Success;
    }
}
