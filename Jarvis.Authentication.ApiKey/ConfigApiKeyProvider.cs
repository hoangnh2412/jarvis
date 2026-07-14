using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.ApiKey;

/// <summary>
/// Provider mặc định validate API key từ header, đọc cấu hình <see cref="AuthenticationApiKeyOption"/> theo realm.
/// </summary>
/// <remarks>
/// <para>Header <c>my-secret</c> → realm <see cref="JarvisAuthenticationSchemes.ApiKey"/> (<c>Default</c>).</para>
/// <para>Header <c>Integration:partner-key</c> → realm <c>Integration</c>, so khớp <c>Key</c> của realm đó.</para>
/// </remarks>
public class ConfigApiKeyProvider(
    IOptionsFactory<AuthenticationApiKeyOption> options,
    IOptions<ApiKeyProviderOptions> providerOptions,
    ILogger<ConfigApiKeyProvider> logger) : IApiKeyProvider
{
    private readonly IOptionsFactory<AuthenticationApiKeyOption> _options = options;
    private readonly IOptions<ApiKeyProviderOptions> _providerOptions = providerOptions;
    private readonly ILogger<ConfigApiKeyProvider> _logger = logger;

    public virtual Task<IApiKey?> ProvideAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult<IApiKey?>(null);

        if (TryParseRealmKey(key, out var realm, out var secret))
            return Task.FromResult<IApiKey?>(Validate(realm, secret, key));

        var defaultRealm = _providerOptions.Value.DefaultRealm;
        return Task.FromResult<IApiKey?>(Validate(defaultRealm, key, key));
    }

    private ApiKeyModel? Validate(string realm, string secret, string rawKey)
    {
        var option = _options.Create(realm);
        if (string.IsNullOrWhiteSpace(option.Key))
        {
            _logger.LogWarning(
                "API key realm {Realm} has empty Key — ConfigApiKeyProvider cannot validate. Use a custom IApiKeyProvider for DB/Redis/MinIO stores.",
                realm);
            return null;
        }

        if (!string.Equals(option.Key, secret, StringComparison.Ordinal))
            return null;

        return new ApiKeyModel(rawKey, realm, null);
    }

    private static bool TryParseRealmKey(string key, out string realm, out string secret)
    {
        var index = key.IndexOf(':');
        if (index <= 0 || index >= key.Length - 1)
        {
            realm = string.Empty;
            secret = string.Empty;
            return false;
        }

        realm = key[..index];
        secret = key[(index + 1)..];
        return true;
    }
}
