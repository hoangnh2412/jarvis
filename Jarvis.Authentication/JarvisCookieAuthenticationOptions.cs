namespace Jarvis.Authentication;

/// <summary>
/// Options cookie authentication dùng chung — bind từ <c>Authentication:Cookie</c>.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> cấu hình path login/logout, thời gian sống, HttpOnly và SameSite cho session cookie.</para>
/// <para><b>Khi nào dùng:</b> host dùng cookie-based login (OpenIddict authorization code, MVC form login).
/// Satellite package đọc options này khi cấu hình cookie middleware.</para>
/// </remarks>
public class JarvisCookieAuthenticationOptions
{
    public string LoginPath { get; set; } = "/account/login";

    public string LogoutPath { get; set; } = "/account/logout";

    public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromHours(1);

    public bool SlidingExpiration { get; set; } = true;

    public bool HttpOnly { get; set; } = true;

    /// <summary>Giá trị SameSite: <c>Lax</c>, <c>Strict</c>, <c>None</c>.</summary>
    public string SameSite { get; set; } = "Lax";
}
