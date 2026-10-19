using Platform.Authentication;

namespace Platform.Authentication.Basic;

/// <summary>
/// Options cấu hình HTTP Basic Authentication — bind từ <c>Authentication:Schemes:Basic:Profiles:{configurationKey}</c>.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> realm challenge (<see cref="Realm"/>) theo profile. User/password qua <see cref="IBasicCredentialProvider"/>.</para>
/// <para><see cref="Users"/> chỉ cho <see cref="ConfigBasicCredentialProvider"/> (test). Production: <c>AddCoreBasic&lt;YourDbProvider&gt;</c>.</para>
/// </remarks>
public class AuthenticationBasicOption
{
    /// <summary>Khóa section mặc định — <c>Authentication:Schemes:Basic:Profiles:Default</c>.</summary>
    public const string DefaultRealm = "Default";

    /// <summary>Scheme HTTP Basic mặc định — <see cref="Platform.Authentication.PlatformAuthenticationSchemes.Basic"/>.</summary>
    public const string DefaultScheme = Platform.Authentication.PlatformAuthenticationSchemes.Basic;

    /// <summary>Nơi lấy user/password cho profile này (profile active: <c>Schemes.Basic.Profile</c>).</summary>
    public AuthenticationCredentialSource Source { get; set; } = AuthenticationCredentialSource.Configuration;

    /// <summary>Realm trả về trong header <c>WWW-Authenticate</c> khi challenge 401.</summary>
    public string Realm { get; set; } = "Platform API";

    /// <summary>Dictionary username → credential (password và roles).</summary>
    public Dictionary<string, BasicUserCredential> Users { get; set; } = new(StringComparer.Ordinal);
}
