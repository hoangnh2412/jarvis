using AspNetCore.Authentication.ApiKey;

namespace UnitTest.Authentication.Helpers;

/// <summary>Storage stub — tests chỉ dùng profile <see cref="Platform.Authentication.AuthenticationCredentialSource.Configuration"/>.</summary>
internal sealed class NullApiKeyStorageProvider : IApiKeyProvider
{
    public Task<IApiKey?> ProvideAsync(string key) => Task.FromResult<IApiKey?>(null);
}
