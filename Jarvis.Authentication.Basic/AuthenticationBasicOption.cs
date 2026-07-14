namespace Jarvis.Authentication.Basic;

/// <summary>
/// Options cấu hình HTTP Basic Authentication — bind từ <c>Authentication:Basic:{configurationKey}</c>.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> khai báo realm challenge (<see cref="Realm"/>) và bảng user/password (<see cref="Users"/>).</para>
/// <para><b>Khi nào dùng:</b> integration đơn giản, dev/test, hoặc service-to-service dùng Basic.
/// Đăng ký qua <c>AddCoreBasic</c>; mỗi scheme map một section config (mặc định <see cref="DefaultRealm"/>).</para>
/// </remarks>
public class AuthenticationBasicOption
{
    /// <summary>Khóa section mặc định — <c>Authentication:Basic:Default</c>.</summary>
    public const string DefaultRealm = "Default";

    /// <summary>Scheme HTTP Basic mặc định.</summary>
    public const string DefaultScheme = "Basic";

    /// <summary>Realm trả về trong header <c>WWW-Authenticate</c> khi challenge 401.</summary>
    public string Realm { get; set; } = "Jarvis API";

    /// <summary>Dictionary username → credential (password và roles).</summary>
    public Dictionary<string, BasicUserCredential> Users { get; set; } = new(StringComparer.Ordinal);
}
