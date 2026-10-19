namespace Platform.Authentication;

/// <summary>Nguồn rule <see cref="PasswordPolicyOptions"/> khi validate mật khẩu.</summary>
public enum PasswordPolicySource
{
    /// <summary>Chỉ dùng <see cref="PasswordPolicyRootOptions.Defaults"/> từ appsettings.</summary>
    Configuration = 0,

    /// <summary>Tra <see cref="IPasswordPolicyProvider"/> — bắt buộc đăng ký implementation trong DI.</summary>
    Storage = 1,
}
