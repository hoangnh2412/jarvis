namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Ambient tenant id for the current async execution flow (HTTP request, background job, etc.).
/// Use <see cref="BeginScope"/> to set and restore; read via <see cref="ITenantIdResolverFactory"/> and connection interceptors.
/// </summary>
public interface ICurrentTenantAccessor
{
    Guid? TenantId { get; }

    /// <summary>
    /// Sets <see cref="TenantId"/> for the current async context until the returned scope is disposed.
    /// </summary>
    IDisposable BeginScope(Guid tenantId);
}
