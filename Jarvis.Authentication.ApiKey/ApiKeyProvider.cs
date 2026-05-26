using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.ApiKey;

public class ApiKeyProvider : IApiKeyProvider
{
    private readonly IOptionsFactory<AuthenticationApiKeyOption> _options;
    private readonly ILogger<IApiKeyProvider> _logger;

    public ApiKeyProvider(
        IOptionsFactory<AuthenticationApiKeyOption> options,
        ILogger<IApiKeyProvider> logger)
    {
        _options = options;
        _logger = logger;
    }

    public virtual async Task<IApiKey?> ProvideAsync(string key)
    {
        await Task.Yield();

        var splited = key.Split(":");
        if (splited.Length != 2)
        {
            _logger.LogError($"API KEY do not contains REALM");
            return null;
        }

        var realm = splited[0];
        var apikey = splited[1];

        var options = _options.Create(realm);
        if (options.Keys.Contains(apikey))
            return new ApiKeyModel(key, realm, null);

        return null;
    }
}