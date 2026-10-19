using Microsoft.Extensions.Configuration;

namespace Platform.Authentication;

/// <summary>
/// Đường dẫn <c>Authentication:Schemes</c> và chọn profile active từ <see cref="AuthenticationSchemeEnableOptions.Profile"/>.
/// </summary>
/// <remarks>
/// <para>Profile không hợp lệ hoặc section trống → fallback <see cref="PlatformAuthenticationSchemes.DefaultProfile"/>.</para>
/// </remarks>
public static class AuthenticationSchemeProfileConfiguration
{
    public const string AuthenticationSchemesConfigurationPath = "Authentication:Schemes";

    public const string ApiKeyDefaultProfile = PlatformAuthenticationSchemes.DefaultProfile;
    public const string BearerDefaultProfile = PlatformAuthenticationSchemes.DefaultProfile;
    public const string BasicDefaultProfile = PlatformAuthenticationSchemes.DefaultProfile;

    public static string SchemeSection(string scheme) =>
        $"{AuthenticationSchemesConfigurationPath}:{scheme}";

    public static string ProfilesSection(string scheme) =>
        $"{AuthenticationSchemesConfigurationPath}:{scheme}:Profiles";

    public static string ProfileSection(string scheme, string profile) =>
        $"{AuthenticationSchemesConfigurationPath}:{scheme}:Profiles:{profile}";

    public static string ResolveApiKeyProfile(AuthenticationRootOptions root, IConfiguration configuration) =>
        ResolveProfile(configuration, "ApiKey", root.Schemes.ApiKey.Profile, ApiKeyDefaultProfile);

    public static string ResolveBearerProfile(AuthenticationRootOptions root, IConfiguration configuration) =>
        ResolveProfile(configuration, "Bearer", root.Schemes.Bearer.Profile, BearerDefaultProfile);

    public static string ResolveBasicProfile(AuthenticationRootOptions root, IConfiguration configuration) =>
        ResolveProfile(configuration, "Basic", root.Schemes.Basic.Profile, BasicDefaultProfile);

    public static string ResolveApiKeyScheme(AuthenticationRootOptions root, IConfiguration configuration) => PlatformAuthenticationSchemes.ApiKey;

    public static string ResolveBasicScheme(AuthenticationRootOptions root, IConfiguration configuration) => PlatformAuthenticationSchemes.Basic;

    public static string ResolveJwtScheme(AuthenticationRootOptions root, IConfiguration configuration) => PlatformAuthenticationSchemes.Bearer;

    /// <summary>
    /// Chọn tên profile bind được dùng cho scheme <paramref name="scheme"/>.
    /// </summary>
    /// <remarks>
    /// Lấy <paramref name="profile"/> từ <c>Schemes.{scheme}.Profile</c> (trống → <paramref name="defaultProfile"/>).
    /// Nếu section <c>Profiles:{candidate}</c> không có field con → thử <paramref name="defaultProfile"/>; vẫn không có → trả <paramref name="defaultProfile"/> (không throw).
    /// </remarks>
    private static string ResolveProfile(
        IConfiguration configuration,
        string scheme,
        string? profile,
        string defaultProfile)
    {
        var candidate = string.IsNullOrWhiteSpace(profile) ? defaultProfile : profile.Trim();
        if (SectionHasValues(configuration, scheme, candidate))
            return candidate;

        if (!string.Equals(candidate, defaultProfile, StringComparison.Ordinal) && SectionHasValues(configuration, scheme, defaultProfile))
            return defaultProfile;

        return defaultProfile;
    }

    /// <summary>
    /// <c>true</c> khi <c>Authentication:Schemes:{scheme}:Profiles:{profile}</c> tồn tại và có ít nhất một key con (vd. <c>KeyName</c>, <c>Source</c>).
    /// </summary>
    private static bool SectionHasValues(IConfiguration configuration, string scheme, string profile)
    {
        var section = configuration.GetSection(ProfileSection(scheme, profile));
        return section.Exists() && section.GetChildren().Any();
    }
}
