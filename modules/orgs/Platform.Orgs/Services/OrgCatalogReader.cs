using Platform.DDD.Domain.Services;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Orgs.Mapping;
using Platform.Orgs.Models;
using Platform.Orgs.Persistence;

namespace Platform.Orgs.Services;

public sealed class OrgCatalogReader<TOrg, TTenant, TReadModel> : IOrgCatalogReader<TReadModel>
    where TOrg : class, IOrgCatalogEntity
    where TTenant : class, ICurrentTenantIdentity
{
    private readonly IOrgStore<TOrg> _store;
    private readonly ICurrentTenant<TTenant> _currentTenant;
    private readonly IOrgCatalogReadMapper<TOrg, TReadModel> _mapper;

    public OrgCatalogReader(
        IOrgStore<TOrg> store,
        ICurrentTenant<TTenant> currentTenant,
        IOrgCatalogReadMapper<TOrg, TReadModel> mapper)
    {
        _store = store;
        _currentTenant = currentTenant;
        _mapper = mapper;
    }

    public async Task<TReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        return await GetAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TReadModel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var normalized = NormalizeCode(code);
        return await GetByCodeAsync(normalized, tenantId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TReadModel>> ListAsync(
        ListOrgsQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        return await ListAsync(tenantId, query?.IsActive, query?.Search, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TReadModel?> GetAsync(
        Guid orgId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _store.FindTrackedAsync(tenantId, orgId, cancellationToken).ConfigureAwait(false);
        return entity is null ? default : _mapper.Map(entity);
    }

    public async Task<TReadModel?> GetByCodeAsync(
        string code,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var entity = await _store.FindByCodeAsync(tenantId, OrgCatalogLimits.NormalizeCode(code), cancellationToken)
            .ConfigureAwait(false);
        return entity is null ? default : _mapper.Map(entity);
    }

    public async Task<IReadOnlyList<TReadModel>> ListAsync(
        Guid tenantId,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var entities = await _store.ListAsync(tenantId, isActive, search, cancellationToken).ConfigureAwait(false);
        return entities.Select(_mapper.Map).ToArray();
    }

    private async Task<Guid> RequireTenantIdAsync(CancellationToken cancellationToken)
    {
        var tenantId = await _currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null || tenantId == Guid.Empty)
            throw new BadRequestException(OrgErrorCode.TenantRequired);

        return tenantId.Value;
    }

    private static string NormalizeCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BadRequestException(OrgErrorCode.InvalidInput);

        var normalized = OrgCatalogLimits.NormalizeCode(code);
        if (normalized.Length > OrgCatalogLimits.CodeMaxLength)
            throw new BadRequestException(OrgErrorCode.InvalidInput);

        return normalized;
    }
}
