using Microsoft.Extensions.Options;
using Platform.Authentication;

namespace Platform.Authentication.ApiKey;

/// <summary>
/// Validate <see cref="AuthenticationApiKeyOption"/> lúc startup — chỉ ràng buộc hành vi scheme (<see cref="AuthenticationApiKeyOption.KeyName"/>).
/// </summary>
/// <remarks>Secret tra cứu qua <see cref="AspNetCore.Authentication.ApiKey.IApiKeyProvider"/> (storage). Không validate <see cref="AuthenticationApiKeyOption.Key"/> trong config.</remarks>
public sealed class AuthenticationApiKeyOptionValidator : IValidateOptions<AuthenticationApiKeyOption>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationApiKeyOption options)
    {
        if (string.IsNullOrWhiteSpace(options.KeyName))
            return ValidateOptionsResult.Fail($"{AuthenticationSchemeProfileConfiguration.ProfileSection("ApiKey", name ?? "profile")}:KeyName is required.");

        return ValidateOptionsResult.Success;
    }
}
