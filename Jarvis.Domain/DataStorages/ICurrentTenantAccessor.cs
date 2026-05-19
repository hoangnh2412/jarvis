namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Ambient tenant id for the current async execution flow (HTTP request, background job, etc.).
/// Use <see cref="BeginScope"/> to set and restore. Read when opening tenant database connections, not from
/// <see cref="Repositories.IUnitOfWork"/> tenant resolution.
/// </summary>
public interface ICurrentTenantAccessor
{
    Guid? TenantId { get; }

    /// <summary>
    /// Sets <see cref="TenantId"/> for the current async context until the returned scope is disposed.
    /// </summary>
    IDisposable BeginScope(Guid tenantId);
}
