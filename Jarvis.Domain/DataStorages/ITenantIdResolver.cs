namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Resolves the current tenant id (<see cref="Guid"/>).
/// Register with <c>AddKeyedScoped&lt;ITenantIdResolver, T&gt;(nameof(T))</c>.
/// </summary>
public interface ITenantIdResolver
{
    Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default);
}
