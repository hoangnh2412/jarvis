using Platform.Authentication;

namespace Platform.Authentication.ApiKey;

/// <summary>
/// Options cấu hình API Key theo realm — bind từ <c>Authentication:Schemes:ApiKey:Profiles:{realm}</c>.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> tên header (<see cref="KeyName"/>) theo profile — secret do <see cref="AspNetCore.Authentication.ApiKey.IApiKeyProvider"/> (storage).</para>
/// <para><b>Profile:</b> mỗi section <c>Authentication:Schemes:ApiKey:Profiles:{profile}</c> (mặc định <c>Default</c>). Prefix <c>profile:secret</c> trong header là tùy provider.</para>
/// </remarks>
public class AuthenticationApiKeyOption
{
    /// <summary>Nơi lấy secret cho profile này.</summary>
    public AuthenticationCredentialSource Source { get; set; } = AuthenticationCredentialSource.Configuration;

    /// <summary>Tên header chứa API key (ví dụ <c>X-API-KEY</c>).</summary>
    public required string KeyName { get; set; }

    /// <summary>Chỉ khi <see cref="Source"/> = <see cref="AuthenticationCredentialSource.Configuration"/> (unit test / integration profile).</summary>
    public string Key { get; set; } = string.Empty;
}
