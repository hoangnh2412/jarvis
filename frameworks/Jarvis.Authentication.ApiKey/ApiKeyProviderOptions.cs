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
}
