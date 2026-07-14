using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.ApiKey;

/// <summary>
/// Validate <see cref="AuthenticationApiKeyOption"/> lúc startup — luôn bắt buộc <c>KeyName</c>;
/// <c>Key</c> chỉ bắt buộc khi <see cref="ApiKeyProviderOptions.RequireConfigKey"/> là <c>true</c>.
/// </summary>
public sealed class AuthenticationApiKeyOptionValidator(
    IOptions<ApiKeyProviderOptions> providerOptions) : IValidateOptions<AuthenticationApiKeyOption>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationApiKeyOption options)
    {
        if (string.IsNullOrWhiteSpace(options.KeyName))
            return ValidateOptionsResult.Fail($"Authentication:ApiKey:{name ?? "realm"}:KeyName is required.");

        if (providerOptions.Value.RequireConfigKey && string.IsNullOrWhiteSpace(options.Key))
            return ValidateOptionsResult.Fail($"Authentication:ApiKey:{name ?? "realm"}:Key is required.");

        return ValidateOptionsResult.Success;
    }
}
