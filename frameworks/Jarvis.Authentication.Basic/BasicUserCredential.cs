namespace Jarvis.Authentication.Basic;

/// <summary>Implementation mặc định — bind từ <see cref="AuthenticationBasicOption.Users"/>.</summary>
public sealed class BasicUserCredential : IBasicUserCredential
{
    /// <summary>Mật khẩu so khớp trực tiếp (plain text trong config — chỉ dùng dev/test).</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Roles gán vào claim <c>ClaimTypes.Role</c> sau khi xác thực thành công.</summary>
    public string[] Roles { get; set; } = [];
}
