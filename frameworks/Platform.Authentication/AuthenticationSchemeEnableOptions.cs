namespace Platform.Authentication;

/// <summary>Bật/tắt một loại scheme và chọn profile dưới <c>Authentication:Schemes:{Kind}:Profiles:{Profile}</c>.</summary>
public class AuthenticationSchemeEnableOptions
{
    /// <summary><c>true</c> = đăng ký handler scheme tương ứng lúc startup (theo host).</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Tên section con (realm). Bỏ trống → ApiKey/Basic: <c>Default</c>, Jwt: <c>Bearer</c>.
    /// Ví dụ ApiKey <c>Integration</c> → <c>Authentication:Schemes:ApiKey:Profiles:Integration</c>.
    /// </summary>
    public string? Profile { get; set; }
}
