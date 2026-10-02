using Platform.DDD.Domain.Repositories;

namespace Platform.DDD.Domain.DataStorages;

/// <summary>
/// Resolves a connection string by lookup name (DbContext name, tenant id, or custom key).
/// Implement in the host for any async source; Platform EF wraps the registration with <c>ICacheService</c> by default.
/// </summary>
public interface ITenantConnectionStringResolver
{
    Task<string?> GetConnectionStringAsync(string name, CancellationToken cancellationToken = default);
}