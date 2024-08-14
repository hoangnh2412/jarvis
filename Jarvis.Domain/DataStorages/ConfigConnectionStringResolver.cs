using Microsoft.Extensions.Configuration;

namespace Jarvis.Domain.DataStorages;

public class ConfigConnectionStringResolver(
    IConfiguration configuration)
    : ITenantConnectionStringResolver
{
    private readonly IConfiguration _configuration = configuration;

    public string Resolve(string? name = null)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        return _configuration.GetConnectionString(name) ?? string.Empty;
    }

    public Task<string> ResolveAsync(string? name = null)
    {
        return Task.FromResult(Resolve(name));
    }
}