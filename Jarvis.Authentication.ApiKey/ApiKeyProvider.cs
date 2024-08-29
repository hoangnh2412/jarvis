using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.ApiKey;

public class ApiKeyProvider : IApiKeyProvider
{
    private readonly AuthenticationApiKeyOption _options;
    private readonly ILogger<IApiKeyProvider> _logger;

    public ApiKeyProvider(
        IOptions<AuthenticationApiKeyOption> options,
        ILogger<IApiKeyProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public virtual async Task<IApiKey?> ProvideAsync(string key)
    {
        await Task.Yield();
        if (_options.Keys.Contains(key))
            return new ApiKeyModel(key, null, null);

        return null;
    }
}