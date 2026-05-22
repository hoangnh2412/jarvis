using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.ApiKey;

public class ApiKeyProvider : IApiKeyProvider
{
    private readonly IOptionsFactory<AuthenticationApiKeyOption> _options;
    private readonly IOptions<ApiKeyProviderOptions> _providerOptions;
    private readonly ILogger<ApiKeyProvider> _logger;

    public ApiKeyProvider(
        IOptionsFactory<AuthenticationApiKeyOption> options,
        IOptions<ApiKeyProviderOptions> providerOptions,
        ILogger<ApiKeyProvider> logger)
    {
        _options = options;
        _providerOptions = providerOptions;
        _logger = logger;
    }

    public virtual Task<IApiKey?> ProvideAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult<IApiKey?>(null);

        if (TryParseRealmKey(key, out var realm, out var secret))
            return Task.FromResult(ValidateRealmKey(realm, secret, key));

        return Task.FromResult(ValidateSingleKey(key));
    }

    private IApiKey? ValidateSingleKey(string key)
    {
        var schemeName = _providerOptions.Value.DefaultSchemeName;
        var option = _options.Create(schemeName);

        if (option.Mode == ApiKeyMode.RealmKey)
        {
            _logger.LogDebug("API key rejected: scheme {Scheme} expects RealmKey format.", schemeName);
            return null;
        }

        if (!option.KeySet.Contains(key))
            return null;

        return new ApiKeyModel(key, schemeName, null);
    }

    private IApiKey? ValidateRealmKey(string realm, string secret, string rawKey)
    {
        var option = _options.Create(realm);
        if (option.Keys.Length == 0 && string.IsNullOrEmpty(option.KeyName))
        {
            _logger.LogDebug("API key realm {Realm} is not configured.", realm);
            return null;
        }

        if (option.Mode == ApiKeyMode.SingleKey)
        {
            if (!option.KeySet.Contains(rawKey))
                return null;

            return new ApiKeyModel(rawKey, realm, null);
        }

        if (!option.KeySet.Contains(secret))
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
