using Jarvis.Authentication;

namespace Jarvis.Authentication.ApiKey;

/// <summary>Options nội bộ — realm mặc định khi header không có prefix <c>realm:</c>.</summary>
public class ApiKeyProviderOptions
{
    /// <summary>
    /// Realm mặc định (trùng section <c>Authentication:ApiKey:Default</c>).
    /// Gán tự động trong <c>AddCoreApiKey</c>.
    /// </summary>
    public string DefaultRealm { get; set; } = JarvisAuthenticationSchemes.ApiKey;

    /// <summary>
    /// Khi <c>true</c> (mặc định với <see cref="ConfigApiKeyProvider"/>), startup bắt buộc <c>Key</c> trong config.
    /// Custom <see cref="AspNetCore.Authentication.ApiKey.IApiKeyProvider"/> (DB/Redis/…) đặt <c>false</c> —
    /// chỉ còn bắt buộc <c>KeyName</c>.
    /// </summary>
    public bool RequireConfigKey { get; set; } = true;
}
