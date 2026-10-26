using Platform.Orgs.Abstractions;
using Platform.Orgs.Models;

namespace Platform.Orgs.Services;

/// <summary>
/// Facade ghi catalog Org trong tenant đang làm việc (<c>ICurrentTenant</c>).
/// Cột host: dùng <see cref="IOrgCatalogReader{TReadModel}"/> + <see cref="Mapping.IOrgCatalogReadMapper{TOrg,TReadModel}"/> (xem <c>UseReadModel</c>).
/// <see cref="OrgDescriptor"/> giữ cho SPI / phản hồi CRUD tối thiểu.
/// </summary>
public interface IOrgManager
{
    Task<OrgDescriptor> CreateAsync(CreateOrgInput input, CancellationToken cancellationToken = default);

    Task<OrgDescriptor> UpdateAsync(Guid id, UpdateOrgInput input, CancellationToken cancellationToken = default);

    Task<OrgDescriptor> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);

    Task<OrgDescriptor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OrgDescriptor?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrgDescriptor>> ListAsync(ListOrgsQuery? query = null, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
