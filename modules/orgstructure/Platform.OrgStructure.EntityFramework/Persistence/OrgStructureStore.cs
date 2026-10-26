using Microsoft.EntityFrameworkCore;
using Platform.DDD.Domain.Entities;
using Platform.DDD.Domain.Repositories;
using Platform.DDD.Domain.Services;
using Platform.OrgStructure;
using Platform.OrgStructure.Persistence;

namespace Platform.OrgStructure.EntityFramework.Persistence;

public sealed class OrgStructureStore<TUnit, TLegalEntity, TJobTitle, TPosition, TUnitOfWork>
    : IOrgStructureStore<TUnit, TLegalEntity, TJobTitle, TPosition>
    where TUnit : class, IOrganizationalUnitEntity
    where TLegalEntity : class, ILegalEntityEntity
    where TJobTitle : class, IJobTitleEntity
    where TPosition : class, IPositionEntity
    where TUnitOfWork : class, IUnitOfWork
{
    private readonly TUnitOfWork _unitOfWork;
    private readonly ICurrentTenantAccessor _tenants;

    public OrgStructureStore(TUnitOfWork unitOfWork, ICurrentTenantAccessor tenants)
    {
        _unitOfWork = unitOfWork;
        _tenants = tenants;
    }

    public async Task<TUnit?> FindUnitAsync(Guid id, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TUnit>(cancellationToken).ConfigureAwait(false);
        return await repo.GetByIdAsync(unit => unit.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TUnit>> ListUnitsAsync(Guid orgId, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TUnit>(cancellationToken).ConfigureAwait(false);
        return await repo.GetQuery().Where(unit => unit.OrgId == orgId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddUnitAsync(TUnit entity, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TUnit>(cancellationToken).ConfigureAwait(false);
        await repo.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TLegalEntity?> FindLegalEntityAsync(Guid id, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TLegalEntity>(cancellationToken).ConfigureAwait(false);
        return await repo.GetByIdAsync(entity => entity.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TLegalEntity>> ListLegalEntitiesAsync(Guid orgId, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TLegalEntity>(cancellationToken).ConfigureAwait(false);
        return await repo.GetQuery().Where(entity => entity.OrgId == orgId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddLegalEntityAsync(TLegalEntity entity, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TLegalEntity>(cancellationToken).ConfigureAwait(false);
        await repo.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TJobTitle?> FindJobTitleAsync(Guid id, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TJobTitle>(cancellationToken).ConfigureAwait(false);
        return await repo.GetByIdAsync(entity => entity.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TJobTitle>> ListJobTitlesAsync(Guid orgId, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TJobTitle>(cancellationToken).ConfigureAwait(false);
        return await repo.GetQuery().Where(entity => entity.OrgId == orgId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddJobTitleAsync(TJobTitle entity, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TJobTitle>(cancellationToken).ConfigureAwait(false);
        await repo.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TPosition?> FindPositionAsync(Guid id, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TPosition>(cancellationToken).ConfigureAwait(false);
        return await repo.GetByIdAsync(entity => entity.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TPosition>> ListPositionsAsync(Guid orgId, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TPosition>(cancellationToken).ConfigureAwait(false);
        return await repo.GetQuery().Where(entity => entity.OrgId == orgId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddPositionAsync(TPosition entity, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync<TPosition>(cancellationToken).ConfigureAwait(false);
        await repo.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _unitOfWork.SaveChangesAsync(cancellationToken);

    private async Task<IRepository<TEntity>> RepositoryAsync<TEntity>(CancellationToken cancellationToken)
        where TEntity : class, IEntity
    {
        var tenantId = _tenants.TenantId
            ?? throw new InvalidOperationException("Thiếu tenant làm việc cho cơ cấu tổ chức.");
        await _unitOfWork.SwitchDbContextAsync(tenantId, cancellationToken).ConfigureAwait(false);
        return await _unitOfWork.GetRepositoryAsync<IRepository<TEntity>>(cancellationToken).ConfigureAwait(false);
    }
}
