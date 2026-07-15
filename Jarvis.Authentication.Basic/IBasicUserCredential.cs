namespace Jarvis.Authentication.Basic;

/// <summary>Thông tin đăng nhập Basic — từ config, DB, hoặc nguồn tùy chỉnh.</summary>
public interface IBasicUserCredential
{
    /// <summary>Mật khẩu để so khớp.</summary>
    string Password { get; }

    /// <summary>Roles gán vào claim <c>ClaimTypes.Role</c> sau khi xác thực thành công.</summary>
    string[] Roles { get; }
}
