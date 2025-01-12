namespace Jarvis.Domain.DataStorages;

public interface ITenantConnectionStringResolver
{
    string GetConnectionString(string? name = null);

    Task<string> GetConnectionStringAsync(string? name = null, CancellationToken cancellationToken = default);
}