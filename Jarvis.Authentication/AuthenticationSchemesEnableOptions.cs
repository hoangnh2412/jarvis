namespace Jarvis.Authentication;

/// <summary>
/// Bật/tắt từng satellite scheme — bind từ <c>Authentication:Schemes</c>.
/// </summary>
/// <remarks>
/// <para><b>Khi nào dùng:</b> host muốn toggle Jwt/ApiKey/Basic qua config mà không sửa code
/// (Sample đọc flags này trong <c>AddSampleAuthentication</c>).</para>
/// </remarks>
public class AuthenticationSchemesEnableOptions
{
    public AuthenticationSchemeEnableOptions Jwt { get; set; } = new();

    public AuthenticationSchemeEnableOptions ApiKey { get; set; } = new();

    public AuthenticationSchemeEnableOptions Basic { get; set; } = new();
}
