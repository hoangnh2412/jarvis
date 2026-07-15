namespace Jarvis.Authentication;

/// <summary>
/// Extension point validate độ mạnh mật khẩu theo <see cref="PasswordPolicyOptions"/>.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> kiểm tra password khi đăng ký/đổi mật khẩu, trả <see cref="PasswordValidationResult"/>.</para>
/// <para><b>Khi nào dùng:</b> override <see cref="DefaultPasswordPolicyValidator"/> trong DI
/// khi cần rule tùy chỉnh (password history, breach check, v.v.).</para>
/// </remarks>
public interface IPasswordPolicyValidator
{
    Task<PasswordValidationResult> ValidateAsync(string password, CancellationToken cancellationToken = default);
}
