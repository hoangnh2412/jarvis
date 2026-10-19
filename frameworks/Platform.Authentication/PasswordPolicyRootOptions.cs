namespace Platform.Authentication;

/// <summary>
/// Cấu hình password policy — bind từ <c>Authentication:PasswordPolicy</c>.
/// </summary>
public class PasswordPolicyRootOptions
{
    /// <summary>
    /// <c>null</c> — không cấu hình <c>Source</c> → rule mặc định của <see cref="PasswordPolicyOptions"/>.
    /// <see cref="PasswordPolicySource.Configuration"/> — <see cref="Defaults"/>; <see cref="PasswordPolicySource.Storage"/> — <see cref="IPasswordPolicyProvider"/>.
    /// </summary>
    public PasswordPolicySource? Source { get; set; }

    /// <summary>Policy khi <see cref="Source"/> là Configuration, hoặc fallback khi storage không có bản ghi.</summary>
    public PasswordPolicyOptions Defaults { get; set; } = new();
}
