namespace Jarvis.Authentication;

/// <summary>
/// Root options cho toàn bộ authentication — bind từ section <c>Authentication</c> trong config.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> xác định loại auth mặc định, default scheme, scheme enable flags,
/// password policy, cookie và expiration options dùng chung.</para>
/// <para><b>Khi nào dùng:</b> khai báo trong <c>appsettings.json</c>; bind tự động qua <c>AddJarvisAuthentication</c>.
/// Satellite packages (Jwt, ApiKey, Basic) đọc section con riêng (<c>Authentication:Jwt</c>, …).</para>
/// </remarks>
public class AuthenticationRootOptions
{
    /// <summary>Loại auth gợi ý (ví dụ <c>Jwt</c>, <c>ApiKey</c>) — host/Sample dùng để quyết định scheme nào bật.</summary>
    public string Type { get; set; } = "Jwt";

    /// <summary>Default authenticate scheme (ví dụ <c>Composite</c>, <c>Bearer</c>, <c>Default</c>).</summary>
    public string? DefaultAuthenticateScheme { get; set; }

    /// <summary>Default challenge scheme khi trả 401.</summary>
    public string? DefaultChallengeScheme { get; set; }

    /// <summary>Flag bật/tắt từng loại scheme con.</summary>
    public AuthenticationSchemesEnableOptions Schemes { get; set; } = new();

    /// <summary>Quy tắc độ mạnh mật khẩu — dùng bởi <see cref="IPasswordPolicyValidator"/>.</summary>
    public PasswordPolicyOptions PasswordPolicy { get; set; } = new();

    /// <summary>Chính sách hết hạn mật khẩu — hook cho flow đăng ký/đổi mật khẩu (OpenIddict tương lai).</summary>
    public PasswordExpirationOptions PasswordExpiration { get; set; } = new();

    /// <summary>Tuỳ chọn cookie authentication dùng chung — mirror ASP.NET Core cookie middleware.</summary>
    public JarvisCookieAuthenticationOptions Cookie { get; set; } = new();
}
