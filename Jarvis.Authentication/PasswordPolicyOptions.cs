namespace Jarvis.Authentication;

/// <summary>
/// Options chính sách độ mạnh mật khẩu — bind từ <c>Authentication:PasswordPolicy</c>.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> quy định độ dài tối thiểu và yêu cầu loại ký tự.</para>
/// <para><b>Khi nào dùng:</b> flow đăng ký/đổi mật khẩu gọi <see cref="IPasswordPolicyValidator"/>.
/// Override validator nếu cần rule phức tạp hơn (history, dictionary check).</para>
/// </remarks>
public class PasswordPolicyOptions
{
    /// <summary>Độ dài tối thiểu. Mặc định 8.</summary>
    public int MinLength { get; set; } = 8;

    public bool RequireDigit { get; set; }

    public bool RequireUppercase { get; set; }

    public bool RequireLowercase { get; set; }

    public bool RequireNonAlphanumeric { get; set; }
}
