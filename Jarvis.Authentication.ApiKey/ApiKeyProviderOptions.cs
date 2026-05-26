namespace Jarvis.Authentication.ApiKey;

/// <summary>Options nội bộ — xác định scheme/realm mặc định khi header không có prefix <c>realm:</c>.</summary>
/// <remarks>
/// <see cref="DefaultSchemeName"/> được gán tự động trong <c>AddCoreApiKey</c>.
/// Host thường không cần bind config riêng cho class này.
/// </remarks>
public class ApiKeyProviderOptions
{
    /// <summary>
    /// Tên scheme/realm dùng khi validate key không có dạng <c>realm:secret</c>.
    /// Mặc định <see cref="JarvisAuthenticationSchemes.ApiKey"/> (<c>"Default"</c>).
    /// </summary>
    public string DefaultSchemeName { get; set; } = JarvisAuthenticationSchemes.ApiKey;
}
