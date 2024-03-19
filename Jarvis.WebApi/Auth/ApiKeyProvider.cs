using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Jarvis.Shared.Options;

namespace Jarvis.WebApi.Auth;

public class ApiKeyProvider : IApiKeyProvider
{
    private readonly AuthenticationOption _options;
    private readonly ILogger<IApiKeyProvider> _logger;

    public ApiKeyProvider(
        IOptions<AuthenticationOption> options,
        ILogger<IApiKeyProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public virtual async Task<IApiKey> ProvideAsync(string key)
    {
        await Task.Yield();
        if (_options.ApiKeys.Contains(key))
            return new ApiKeyModel(key, null, null);

        return null;
    }
}