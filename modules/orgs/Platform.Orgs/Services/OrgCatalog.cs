using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Orgs.Abstractions;
using Platform.Orgs.Persistence;

namespace Platform.Orgs.Services;

/// <summary>Đọc catalog theo tenant caller truyền vào.</summary>
public sealed class OrgCatalog<TOrg> : IOrgCatalog
    where TOrg : class, IOrgManagementEntity
{
    private readonly IOrgStore<TOrg> _store;

    public OrgCatalog(IOrgStore<TOrg> store)
    {
        _store = store;
    }

    public async Task<OrgDescriptor?> GetAsync(
        Guid orgId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _store.FindTrackedAsync(tenantId, orgId, cancellationToken).ConfigureAwait(false);
        return entity is null ? null : OrgDescriptorMapper.ToDescriptor(entity);
    }

    public async Task<OrgDescriptor?> GetByCodeAsync(
        string code,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var entity = await _store.FindByCodeAsync(tenantId, OrgCatalogLimits.NormalizeCode(code), cancellationToken)
            .ConfigureAwait(false);
        return entity is null ? null : OrgDescriptorMapper.ToDescriptor(entity);
    }

    public async Task<IReadOnlyList<OrgDescriptor>> ListAsync(
        Guid tenantId,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var entities = await _store.ListAsync(tenantId, isActive, search, cancellationToken).ConfigureAwait(false);
        return entities.Select(OrgDescriptorMapper.ToDescriptor).ToArray();
    }

    public async Task ValidateForAssignmentAsync(
        Guid orgId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _store.FindTrackedAsync(tenantId, orgId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
            throw new NotFoundException(OrgErrorCode.NotFound);

        if (!entity.IsActive)
            throw new ConflictException(OrgErrorCode.Inactive);
    }
}
