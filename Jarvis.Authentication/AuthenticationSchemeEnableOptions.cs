namespace Jarvis.Authentication;

/// <summary>Flag bật/tắt một loại authentication scheme.</summary>
public class AuthenticationSchemeEnableOptions
{
    /// <summary><c>true</c> = host đăng ký scheme tương ứng lúc startup.</summary>
    public bool Enabled { get; set; }
}
