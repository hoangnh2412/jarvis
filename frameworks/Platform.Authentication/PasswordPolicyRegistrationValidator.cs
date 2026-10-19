using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Platform.Authentication;

/// <summary>
/// <see cref="PasswordPolicySource.Storage"/> yêu cầu <see cref="IPasswordPolicyProvider"/> thật trong DI.
/// </summary>
public sealed class PasswordPolicyRegistrationValidator(IServiceProvider serviceProvider)
    : IValidateOptions<AuthenticationRootOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationRootOptions options)
    {
        if (options.PasswordPolicy.Source != PasswordPolicySource.Storage)
            return ValidateOptionsResult.Success;

        var provider = serviceProvider.GetService<IPasswordPolicyProvider>();
        if (provider is null or NullPasswordPolicyProvider)
        {
            return ValidateOptionsResult.Fail(
                "Authentication:PasswordPolicy:Source is Storage. Register IPasswordPolicyProvider before AddPlatformAuthentication (NullPasswordPolicyProvider is not sufficient).");
        }

        return ValidateOptionsResult.Success;
    }
}
