namespace Jarvis.Authentication.ApiKey;

/// <summary>
/// Options cấu hình API Key theo realm — bind từ <c>Authentication:ApiKey:{realm}</c>.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> khai báo tên header (<see cref="KeyName"/>), định dạng key (<see cref="Mode"/>),
/// và danh sách secret hợp lệ (<see cref="Keys"/>).</para>
/// <para><b>Khi nào dùng:</b> mỗi section con dưới <c>Authentication:ApiKey</c> map một realm/scheme
/// (ví dụ <c>Default</c>, <c>Integration</c>). Đăng ký tự động qua <c>AddCoreApiKey</c> — host chỉ cần
/// khai báo trong <c>appsettings</c>, không instantiate class này trực tiếp.</para>
/// </remarks>
public class AuthenticationApiKeyOption
{
    /// <summary>Tên header chứa API key (ví dụ <c>X-API-KEY</c>).</summary>
    public required string KeyName { get; set; }

    /// <summary>Định dạng giá trị header — xem <see cref="ApiKeyMode"/>.</summary>
    public ApiKeyMode Mode { get; set; } = ApiKeyMode.SingleKey;

    /// <summary>Danh sách secret hợp lệ cho realm này.</summary>
    public string[] Keys { get; set; } = [];

    /// <summary>HashSet tra cứu nhanh — được build bởi <see cref="AuthenticationApiKeyPostConfigureOptions"/>.</summary>
    internal HashSet<string> KeySet { get; set; } = new(StringComparer.Ordinal);
}
