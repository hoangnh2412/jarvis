using Microsoft.AspNetCore.Authentication;

namespace Platform.Authentication;

/// <summary>
/// Map <see cref="AuthenticationRootOptions"/> → <see cref="AuthenticationOptions"/>.
/// </summary>
/// <remarks>
/// Mỗi <c>Default*Scheme</c> là một tên scheme. <see cref="AuthenticationRootOptions.DefaultScheme"/> chỉ là fallback khi property tương ứng không set (giống ASP.NET Core).
/// </remarks>
public static class AuthenticationSchemeResolution
{
    public static void ApplyTo(AuthenticationOptions options, AuthenticationRootOptions root)
    {
        if (!string.IsNullOrWhiteSpace(root.DefaultScheme))
            options.DefaultScheme = root.DefaultScheme;

        options.DefaultAuthenticateScheme = ResolveAuthenticateScheme(root);
        options.DefaultChallengeScheme = ResolveChallengeScheme(root);

        if (!string.IsNullOrWhiteSpace(root.DefaultSignInScheme))
            options.DefaultSignInScheme = root.DefaultSignInScheme;

        if (!string.IsNullOrWhiteSpace(root.DefaultSignOutScheme))
            options.DefaultSignOutScheme = root.DefaultSignOutScheme;

        if (!string.IsNullOrWhiteSpace(root.DefaultForbidScheme))
            options.DefaultForbidScheme = root.DefaultForbidScheme;

        options.RequireAuthenticatedSignIn = root.RequireAuthenticatedSignIn;
    }

    public static string ResolveAuthenticateScheme(AuthenticationRootOptions root)
    {
        if (!string.IsNullOrWhiteSpace(root.DefaultAuthenticateScheme))
            return root.DefaultAuthenticateScheme.Trim();

        if (!string.IsNullOrWhiteSpace(root.DefaultScheme))
            return root.DefaultScheme.Trim();

        return MapAuthenticationTypeToScheme(root.Type);
    }

    public static string ResolveChallengeScheme(AuthenticationRootOptions root)
    {
        if (!string.IsNullOrWhiteSpace(root.DefaultChallengeScheme))
            return root.DefaultChallengeScheme.Trim();

        if (!string.IsNullOrWhiteSpace(root.DefaultScheme))
            return root.DefaultScheme.Trim();

        return MapAuthenticationTypeToScheme(root.Type);
    }

    public static string MapAuthenticationTypeToScheme(string? type) =>
        (type ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "jwt" => PlatformAuthenticationSchemes.Bearer,
            "basic" => PlatformAuthenticationSchemes.Basic,
            "apikey" => PlatformAuthenticationSchemes.ApiKey,
            _ => PlatformAuthenticationSchemes.ApiKey,
        };

    public static bool IsSchemeEnabledForRegistration(AuthenticationRootOptions root, string scheme)
    {
        if (string.Equals(scheme, PlatformAuthenticationSchemes.Basic, StringComparison.Ordinal))
            return root.Schemes.Basic.Enabled;

        if (string.Equals(scheme, PlatformAuthenticationSchemes.Bearer, StringComparison.Ordinal))
            return root.Schemes.Bearer.Enabled;

        if (string.Equals(scheme, PlatformAuthenticationSchemes.ApiKey, StringComparison.Ordinal))
            return root.Schemes.ApiKey.Enabled;

        return true;
    }
}
