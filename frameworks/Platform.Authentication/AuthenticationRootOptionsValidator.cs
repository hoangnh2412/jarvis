using Microsoft.Extensions.Options;

namespace Platform.Authentication;

/// <summary>
/// Validate <see cref="AuthenticationRootOptions"/> lúc startup.
/// </summary>
public sealed class AuthenticationRootOptionsValidator : IValidateOptions<AuthenticationRootOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationRootOptions options)
    {
        if (!HasExplicitDefaultSchemes(options) && string.IsNullOrWhiteSpace(options.Type))
            return ValidateOptionsResult.Fail(
                "Authentication: set DefaultScheme, DefaultAuthenticateScheme, DefaultChallengeScheme, or Type.");

        var authenticate = AuthenticationSchemeResolution.ResolveAuthenticateScheme(options);
        if (!AuthenticationSchemeResolution.IsSchemeEnabledForRegistration(options, authenticate))
            return ValidateOptionsResult.Fail(
                $"Authentication: default authenticate scheme '{authenticate}' is not enabled (check Schemes).");

        var challenge = AuthenticationSchemeResolution.ResolveChallengeScheme(options);
        if (!AuthenticationSchemeResolution.IsSchemeEnabledForRegistration(options, challenge))
            return ValidateOptionsResult.Fail(
                $"Authentication: default challenge scheme '{challenge}' is not enabled (check Schemes).");

        foreach (var (label, scheme) in EnumerateOptionalSchemes(options))
        {
            if (string.IsNullOrWhiteSpace(scheme))
                continue;

            if (!AuthenticationSchemeResolution.IsSchemeEnabledForRegistration(options, scheme))
                return ValidateOptionsResult.Fail(
                    $"Authentication: {label} scheme '{scheme}' is not enabled (check Schemes).");
        }

        return ValidateOptionsResult.Success;
    }

    private static bool HasExplicitDefaultSchemes(AuthenticationRootOptions options) =>
        !string.IsNullOrWhiteSpace(options.DefaultScheme)
        || !string.IsNullOrWhiteSpace(options.DefaultAuthenticateScheme)
        || !string.IsNullOrWhiteSpace(options.DefaultChallengeScheme)
        || !string.IsNullOrWhiteSpace(options.DefaultSignInScheme)
        || !string.IsNullOrWhiteSpace(options.DefaultSignOutScheme)
        || !string.IsNullOrWhiteSpace(options.DefaultForbidScheme);

    private static IEnumerable<(string Label, string? Scheme)> EnumerateOptionalSchemes(AuthenticationRootOptions options)
    {
        yield return ("DefaultSignIn", options.DefaultSignInScheme);
        yield return ("DefaultSignOut", options.DefaultSignOutScheme);
        yield return ("DefaultForbid", options.DefaultForbidScheme);
    }
}
