using Microsoft.Extensions.Configuration;

namespace Jarvis.Domain.DataStorages;

public class ConfigConnectionStringResolver(
    IConfiguration configuration)
    : ITenantConnectionStringResolver
{
    private readonly IConfiguration _configuration = configuration;

    public string GetConnectionString(string? name = null)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        return _configuration.GetConnectionString(name) ?? string.Empty;
    }

    public Task<string> GetConnectionStringAsync(string? name = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetConnectionString(name));
    }
}