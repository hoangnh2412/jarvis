namespace Jarvis.Domain.DataStorages;

public interface ITenantConnectionStringResolver
{
    Task<string?> GetConnectionStringAsync(string name, CancellationToken cancellationToken = default);
}