namespace Platform.Authentication;

/// <summary>
/// Extension point validate độ mạnh mật khẩu theo <see cref="PasswordPolicyOptions"/>.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> kiểm tra password khi đăng ký/đổi mật khẩu, trả <see cref="PasswordValidationResult"/>.</para>
/// <para><b>Khi nào dùng:</b> override <see cref="PlatformPasswordPolicyValidator"/> hoặc đăng ký <see cref="IPasswordPolicyProvider"/>
/// khi <see cref="PasswordPolicySource.Storage"/>.</para>
/// </remarks>
public interface IPasswordPolicyValidator
{
    Task<PasswordValidationResult> ValidateAsync(
        string password,
        PasswordPolicyScope? scope = null,
        CancellationToken cancellationToken = default);
}
