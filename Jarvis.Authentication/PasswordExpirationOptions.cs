namespace Jarvis.Authentication;

/// <summary>
/// Options hết hạn mật khẩu — bind từ <c>Authentication:PasswordExpiration</c>.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> giới hạn tuổi mật khẩu và số ngày cảnh báo trước khi hết hạn.</para>
/// <para><b>Khi nào dùng:</b> tích hợp với flow đăng nhập/đổi mật khẩu (OpenIddict hoặc custom identity).
/// Validator mặc định chưa enforce — host implement hook riêng.</para>
/// </remarks>
public class PasswordExpirationOptions
{
    /// <summary>Số ngày tối đa trước khi bắt buộc đổi mật khẩu. <c>0</c> = không giới hạn.</summary>
    public int MaxAgeDays { get; set; }

    /// <summary>Số ngày trước hết hạn để hiển thị cảnh báo.</summary>
    public int WarnBeforeDays { get; set; }
}
