namespace Platform.Authentication;

/// <summary>
/// Bật/tắt từng satellite scheme — bind từ <c>Authentication:Schemes</c>.
/// </summary>
/// <remarks>
/// <para><b>Enabled</b> — đăng ký handler. <b>Profile</b> — profile active; <c>Source</c> nằm trong từng mục <c>Profiles</c>.</para>
/// </remarks>
public class AuthenticationSchemesEnableOptions
{
    public AuthenticationSchemeEnableOptions Bearer { get; set; } = new();

    public AuthenticationSchemeEnableOptions ApiKey { get; set; } = new();

    public AuthenticationSchemeEnableOptions Basic { get; set; } = new();
}
