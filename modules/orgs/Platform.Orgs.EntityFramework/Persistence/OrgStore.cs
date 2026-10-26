using Microsoft.EntityFrameworkCore;
using Platform.DDD.Domain.Repositories;
using Platform.Orgs;
using Platform.Orgs.Persistence;

namespace Platform.Orgs.EntityFramework.Persistence;

/// <summary>Store catalog qua <see cref="IUnitOfWork"/> và <see cref="IRepository{TEntity}"/>.</summary>
public sealed class OrgStore<TOrg, TUnitOfWork>(TUnitOfWork unitOfWork) : IOrgStore<TOrg>
    where TOrg : class, IOrgCatalogEntity
    where TUnitOfWork : class, IUnitOfWork
{
    private readonly TUnitOfWork _unitOfWork = unitOfWork;

    public async Task<TOrg?> FindTrackedAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync(tenantId, cancellationToken).ConfigureAwait(false);
        return await repo.GetByIdAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TOrg?> FindByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync(tenantId, cancellationToken).ConfigureAwait(false);
        return await repo.GetByIdAsync(
            x => x.TenantId == tenantId && x.Code == code,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> CodeExistsAsync(
        Guid tenantId,
        string code,
        Guid? exceptId,
        CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync(tenantId, cancellationToken).ConfigureAwait(false);
        return await repo.GetQuery().AnyAsync(
            x => x.TenantId == tenantId
                && x.Code == code
                && (exceptId == null || x.Id != exceptId),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TOrg>> ListAsync(
        Guid tenantId,
        bool? isActive,
        string? search,
        CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync(tenantId, cancellationToken).ConfigureAwait(false);
        var query = repo.GetQuery().Where(x => x.TenantId == tenantId);

        if (isActive is bool active)
            query = query.Where(x => x.IsActive == active);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }

        return await query.OrderBy(x => x.Code).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddAsync(TOrg entity, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync(entity.TenantId, cancellationToken).ConfigureAwait(false);
        await repo.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(TOrg entity, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync(entity.TenantId, cancellationToken).ConfigureAwait(false);
        await repo.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _unitOfWork.SaveChangesAsync(cancellationToken);

    private async Task<IRepository<TOrg>> RepositoryAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await _unitOfWork.SwitchDbContextAsync(tenantId, cancellationToken).ConfigureAwait(false);
        return await _unitOfWork.GetRepositoryAsync<IRepository<TOrg>>(cancellationToken).ConfigureAwait(false);
    }
}
