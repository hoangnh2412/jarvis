namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Resolves the connection string for a <c>DbContext</c> using <see cref="ITenantIdResolverFactory"/>
/// and a keyed <see cref="ITenantConnectionStringResolver"/>.
/// </summary>
public interface ITenantConnectionStringResolverFactory
{
    Task<string?> GetConnectionStringAsync(Type dbContextType, CancellationToken cancellationToken = default);
}
