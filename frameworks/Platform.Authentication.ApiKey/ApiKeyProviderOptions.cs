using Platform.Authentication;

namespace Platform.Authentication.ApiKey;

/// <summary>Options nội bộ — realm mặc định khi header không có prefix <c>realm:</c>.</summary>
public class ApiKeyProviderOptions
{
    /// <summary>
    /// Realm mặc định (trùng profile <c>Authentication:Schemes:ApiKey:Profiles:Default</c>).
    /// Gán tự động trong <c>AddCoreApiKey</c>.
    /// </summary>
    public string DefaultRealm { get; set; } = PlatformAuthenticationSchemes.DefaultProfile;
}
