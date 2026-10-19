using Microsoft.Extensions.Options;

namespace Platform.Authentication;

/// <summary>
/// Validate mật khẩu theo <see cref="PasswordPolicyRootOptions.Source"/> — appsettings hoặc <see cref="IPasswordPolicyProvider"/>.
/// </summary>
public sealed class PlatformPasswordPolicyValidator(
    IOptions<AuthenticationRootOptions> rootOptions,
    IPasswordPolicyProvider policyProvider,
    IPasswordPolicyScopeProvider scopeProvider) : IPasswordPolicyValidator
{
    public async Task<PasswordValidationResult> ValidateAsync(
        string password,
        PasswordPolicyScope? scope = null,
        CancellationToken cancellationToken = default)
    {
        var settings = rootOptions.Value.PasswordPolicy;
        PasswordPolicyOptions? fromStorage = null;
        if (settings.Source == PasswordPolicySource.Storage)
        {
            fromStorage = await policyProvider.GetPolicyAsync(
                scope ?? await scopeProvider.GetScopeAsync(cancellationToken),
                cancellationToken);
        }

        var policy = PasswordPolicyConfiguration.Resolve(settings, fromStorage);
        return PasswordPolicyRules.Validate(password, policy);
    }
}
