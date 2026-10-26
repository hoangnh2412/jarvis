using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Tenants.Models;
using Platform.Tenants.Persistence;

namespace Platform.Tenants.Services;

/// <summary>Facade catalog tenant trên Master DB. Không có hard delete.</summary>
public interface ITenantManager
{
    Task<TenantDescriptor> CreateAsync(CreateTenantInput input, CancellationToken cancellationToken = default);

    Task<TenantDescriptor> UpdateAsync(Guid id, UpdateTenantInput input, CancellationToken cancellationToken = default);

    Task<TenantDescriptor> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);

    Task<TenantDescriptor> SetParentAsync(Guid id, Guid? parentTenantId, CancellationToken cancellationToken = default);

    Task<TenantDescriptor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TenantDescriptor?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<string> GetConnectionStringAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantDescriptor>> ListAsync(ListTenantsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Bản thân tenant và các tenant thấp hơn. Không gồm anh em hay tenant cao hơn.</summary>
    Task<IReadOnlyList<TenantDescriptor>> ListSubtreeAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
