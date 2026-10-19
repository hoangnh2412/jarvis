using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Options;
using Platform.Authentication;

namespace Platform.Authentication.ApiKey;

/// <summary>
/// Route API key validation theo <see cref="AuthenticationApiKeyOption.Source"/> từng profile.
/// </summary>
public sealed class ProfileSourcedApiKeyProvider<TStorage>(
    IOptionsFactory<AuthenticationApiKeyOption> optionsFactory,
    IOptions<ApiKeyProviderOptions> providerOptions,
    ConfigApiKeyProvider configurationProvider,
    TStorage storageProvider)
    : IApiKeyProvider
    where TStorage : class, IApiKeyProvider
{
    public Task<IApiKey?> ProvideAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult<IApiKey?>(null);

        var realm = ResolveRealm(key);
        var source = optionsFactory.Create(realm).Source;
        return source == AuthenticationCredentialSource.Storage
            ? storageProvider.ProvideAsync(key)
            : configurationProvider.ProvideAsync(key);
    }

    private string ResolveRealm(string key)
    {
        var index = key.IndexOf(':');
        if (index > 0 && index < key.Length - 1)
            return key[..index];

        return providerOptions.Value.DefaultRealm;
    }
}
