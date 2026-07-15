namespace Jarvis.Authentication.ApiKey;

/// <summary>
/// Options cấu hình API Key theo realm — bind từ <c>Authentication:ApiKey:{realm}</c>.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> khai báo tên header (<see cref="KeyName"/>) và secret (<see cref="Key"/>).</para>
/// <para><b>Khi nào dùng:</b> mỗi section con dưới <c>Authentication:ApiKey</c> là một realm
/// (mặc định <c>Default</c>). Header không có <c>realm:</c> → realm mặc định <c>Default</c>.</para>
/// </remarks>
public class AuthenticationApiKeyOption
{
    /// <summary>Tên header chứa API key (ví dụ <c>X-API-KEY</c>).</summary>
    public required string KeyName { get; set; }

    /// <summary>
    /// Secret hợp lệ cho realm này — bắt buộc khi dùng <see cref="ConfigApiKeyProvider"/>.
    /// Custom <see cref="AspNetCore.Authentication.ApiKey.IApiKeyProvider"/> có thể để trống.
    /// </summary>
    public string Key { get; set; } = string.Empty;
}
