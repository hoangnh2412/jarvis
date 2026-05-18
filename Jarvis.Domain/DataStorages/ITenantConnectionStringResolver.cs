using Jarvis.Domain.Repositories;

namespace Jarvis.Domain.DataStorages;

public interface ITenantConnectionStringResolver
{
    Task<string?> GetConnectionStringAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// Marker for type-safe registration with a specific <see cref="IStorageContext"/> (see <c>AddCoreDbContext&lt;TDb,TConn&gt;</c>).
/// Runtime dispatch uses non-generic <see cref="ITenantConnectionStringResolver"/>.
/// </summary>
public interface ITenantConnectionStringResolver<TDbContext> : ITenantConnectionStringResolver
    where TDbContext : IStorageContext
{
}
