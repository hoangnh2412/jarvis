namespace Platform.Authentication;

/// <summary>
/// Root options cho toàn bộ authentication — bind từ section <c>Authentication</c> trong config.
/// </summary>
/// <remarks>
/// <para><b>Schemes:</b> <see cref="AuthenticationSchemesEnableOptions"/> — <c>Enabled</c>, <c>Profile</c> và section profile con
/// (<c>Authentication:Schemes:ApiKey:Profiles:{Profile}</c>, …). Host đăng ký <c>AddCore*</c> theo config; xem <c>docs/guides/authentication/appsettings.md</c>.</para>
/// <para><b>Default schemes:</b> map 1:1 sang <see cref="Microsoft.AspNetCore.Authentication.AuthenticationOptions"/> (xem từng property).
/// <see cref="Type"/> chỉ fallback khi không có <see cref="DefaultScheme"/> / <c>Default*Scheme</c> cụ thể.</para>
/// </remarks>
public class AuthenticationRootOptions
{
    /// <summary>Fallback map scheme khi các <c>Default*Scheme</c> cụ thể bỏ trống (vd. <c>Jwt</c> → <c>Bearer</c>).</summary>
    public string? Type { get; set; }

    /// <summary>
    /// Fallback chung cho các thao tác khi <c>DefaultAuthenticateScheme</c> / <c>DefaultChallengeScheme</c> / … không được set trong config runtime.
    /// </summary>
    /// <remarks>
    /// <para><b>Câu hỏi:</b> “Scheme dự phòng khi một thao tác không có default riêng?”</para>
    /// <para>Map <see cref="Microsoft.AspNetCore.Authentication.AuthenticationOptions.DefaultScheme"/>.</para>
    /// <para>Platform: nếu <see cref="DefaultAuthenticateScheme"/> trống, resolve authenticate có thể dùng giá trị này trước <see cref="Type"/>.</para>
    /// </remarks>
    public string? DefaultScheme { get; set; }

    /// <summary>
    /// Scheme mặc định khi ASP.NET Core <b>xác thực</b> request (<c>UseAuthentication</c>, <c>[Authorize]</c>, <c>HttpContext.User</c>).
    /// </summary>
    /// <remarks>
    /// <para><b>Câu hỏi cấu hình này trả lời:</b> “Với endpoint có <c>[Authorize]</c> (hoặc middleware cần user), hệ thống dùng <b>loại credential nào</b> và <b>handler scheme nào</b> để đọc header/token và gắn <c>HttpContext.User</c>?”</para>
    /// <para>Map sang <see cref="Microsoft.AspNetCore.Authentication.AuthenticationOptions.DefaultAuthenticateScheme"/> lúc <c>AddPlatformAuthentication</c>.</para>
    /// <para>Ví dụ: <see cref="PlatformAuthenticationSchemes.Bearer"/> / <see cref="PlatformAuthenticationSchemes.Basic"/> / <see cref="PlatformAuthenticationSchemes.ApiKey"/>.</para>
    /// <para>Bỏ trống property này → <see cref="AuthenticationSchemeResolution"/> fallback <see cref="DefaultScheme"/> rồi <see cref="Type"/>.</para>
    /// <para><b>ASP.NET Core thuần:</b> <see cref="Microsoft.AspNetCore.Authentication.AuthenticationOptions.DefaultAuthenticateScheme"/> là <b>tên scheme bạn đăng ký</b> (<c>AddJwtBearer("Bearer", …)</c>, …). Nếu <b>không gán</b> (null) thì framework <b>không</b> tự chọn scheme tên <c>Default</c>; <c>[Authorize]</c> thường lỗi hoặc không tự authenticate cho đến khi host set default hoặc attribute chỉ rõ scheme.</para>
    /// <para>API thường đặt cùng giá trị với <see cref="DefaultChallengeScheme"/>.</para>
    /// </remarks>
    public string? DefaultAuthenticateScheme { get; set; }

    /// <summary>
    /// Scheme mặc định khi framework <b>challenge</b> client (chưa đăng nhập / auth thất bại — thường 401 và header <c>WWW-Authenticate</c>).
    /// </summary>
    /// <remarks>
    /// <para><b>Câu hỏi cấu hình này trả lời:</b> “Khi request <b>chưa được xác thực</b> hoặc <b>không đủ quyền</b>, API trả lỗi theo <b>chuẩn nào</b> để client biết phải gửi JWT, ApiKey hay Basic (header <c>WWW-Authenticate</c>, 401, …)?”</para>
    /// <para>Map sang <see cref="Microsoft.AspNetCore.Authentication.AuthenticationOptions.DefaultChallengeScheme"/>.</para>
    /// <para>Khác <see cref="DefaultAuthenticateScheme"/>: authenticate = “đọc credential và gắn user”; challenge = “báo client cần đăng nhập theo cách nào”.</para>
    /// <para>API thường đặt cùng giá trị với <see cref="DefaultAuthenticateScheme"/>; tách khi host cần (hiếm).</para>
    /// </remarks>
    public string? DefaultChallengeScheme { get; set; }

    /// <summary>
    /// Scheme mặc định cho <c>SignInAsync</c> (cookie session, OpenIdConnect, …).
    /// </summary>
    /// <remarks>
    /// <para><b>Câu hỏi:</b> “Sau login thành công, ghi trạng thái đăng nhập qua scheme nào?”</para>
    /// <para>Map <see cref="Microsoft.AspNetCore.Authentication.AuthenticationOptions.DefaultSignInScheme"/>.</para>
    /// </remarks>
    public string? DefaultSignInScheme { get; set; }

    /// <summary>
    /// Scheme mặc định cho <c>SignOutAsync</c>.
    /// </summary>
    /// <remarks>
    /// <para><b>Câu hỏi:</b> “Đăng xuất xóa cookie / session scheme nào?”</para>
    /// <para>Map <see cref="Microsoft.AspNetCore.Authentication.AuthenticationOptions.DefaultSignOutScheme"/>.</para>
    /// </remarks>
    public string? DefaultSignOutScheme { get; set; }

    /// <summary>
    /// Scheme mặc định cho <c>ForbidAsync</c> (đã auth nhưng không đủ quyền — thường 403).
    /// </summary>
    /// <remarks>
    /// <para><b>Câu hỏi:</b> “Forbidden trả về theo handler nào?”</para>
    /// <para>Map <see cref="Microsoft.AspNetCore.Authentication.AuthenticationOptions.DefaultForbidScheme"/>.</para>
    /// </remarks>
    public string? DefaultForbidScheme { get; set; }

    /// <summary>
    /// Từ chối <c>SignInAsync</c> khi <see cref="System.Security.Claims.ClaimsIdentity.IsAuthenticated"/> = false.
    /// </summary>
    /// <remarks>Map <see cref="Microsoft.AspNetCore.Authentication.AuthenticationOptions.RequireAuthenticatedSignIn"/>.</remarks>
    public bool RequireAuthenticatedSignIn { get; set; }

    /// <summary>Flag bật/tắt từng loại scheme con.</summary>
    public AuthenticationSchemesEnableOptions Schemes { get; set; } = new();

    /// <summary>Password policy — appsettings hoặc storage (<see cref="PasswordPolicyRootOptions.Source"/>).</summary>
    public PasswordPolicyRootOptions PasswordPolicy { get; set; } = new();

    /// <summary>Chính sách hết hạn mật khẩu — hook cho flow đăng ký/đổi mật khẩu (OpenIddict tương lai).</summary>
    public PasswordExpirationOptions PasswordExpiration { get; set; } = new();

    /// <summary>Tuỳ chọn cookie authentication dùng chung — mirror ASP.NET Core cookie middleware.</summary>
    public PlatformCookieAuthenticationOptions Cookie { get; set; } = new();
}
