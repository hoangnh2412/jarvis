using Microsoft.Extensions.Configuration;

namespace Jarvis.Domain.DataStorages;

public class ConfigConnectionStringResolver(
    IConfiguration configuration)
    : ITenantConnectionStringResolver
{
    private readonly IConfiguration _configuration = configuration;

    public Task<string?> GetConnectionStringAsync(string name, CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString(name);
        return Task.FromResult(string.IsNullOrWhiteSpace(connectionString) ? null : connectionString);
    }
}
