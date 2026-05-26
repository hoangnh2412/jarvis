using Jarvis.Domain.Repositories;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Resolves a connection string by lookup name (DbContext name, tenant id, or custom key).
/// Implement in the host for any async source; Jarvis EF wraps the registration with <c>ICacheService</c> by default.
/// </summary>
public interface ITenantConnectionStringResolver
{
    Task<string?> GetConnectionStringAsync(string name, CancellationToken cancellationToken = default);
}