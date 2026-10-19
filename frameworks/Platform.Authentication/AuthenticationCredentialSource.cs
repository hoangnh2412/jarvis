namespace Platform.Authentication;

/// <summary>
/// Nơi lấy dữ liệu xác thực — đồng bộ với <see cref="PasswordPolicySource"/> (Configuration / Storage).
/// </summary>
/// <remarks>
/// <c>Profiles</c> trong appsettings giữ hành vi scheme (<c>KeyName</c>, <c>Realm</c>, JWT validation).
/// <see cref="Configuration"/> — secret/user/signing key trong config (test, dev).
/// <see cref="Storage"/> — <c>IApiKeyProvider</c> / <c>IBasicCredentialProvider</c> / host (DB, vault).
/// </remarks>
public enum AuthenticationCredentialSource
{
    Configuration = 0,

    Storage = 1,
}
