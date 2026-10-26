using Microsoft.EntityFrameworkCore;
using Platform.DDD.Domain.Repositories;
using Platform.Tenants.Persistence;

namespace Platform.Tenants.EntityFramework.Persistence;

/// <summary>Store catalog tenant qua repository của Unit of Work Master.</summary>
public sealed class TenantStore<TTenant, TUnitOfWork>(TUnitOfWork unitOfWork) : ITenantStore<TTenant>
    where TTenant : class, ITenantCatalogEntity
    where TUnitOfWork : class, IUnitOfWork
{
    private readonly TUnitOfWork _unitOfWork = unitOfWork;

    public async Task<TTenant?> FindTrackedAsync(Guid id, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync(cancellationToken).ConfigureAwait(false);
        return await repo.GetByIdAsync(tenant => tenant.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TTenant?> FindByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync(cancellationToken).ConfigureAwait(false);
        return await repo.GetByIdAsync(tenant => tenant.Code == code, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? exceptId, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync(cancellationToken).ConfigureAwait(false);
        return await repo.GetQuery().AnyAsync(
            tenant => tenant.Code == code && (exceptId == null || tenant.Id != exceptId),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TTenant>> ListAllAsync(CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync(cancellationToken).ConfigureAwait(false);
        return await repo.GetQuery().OrderBy(tenant => tenant.Code).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddAsync(TTenant entity, CancellationToken cancellationToken)
    {
        var repo = await RepositoryAsync(cancellationToken).ConfigureAwait(false);
        await repo.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _unitOfWork.SaveChangesAsync(cancellationToken);

    private async Task<IRepository<TTenant>> RepositoryAsync(CancellationToken cancellationToken) =>
        await _unitOfWork.GetRepositoryAsync<IRepository<TTenant>>(cancellationToken).ConfigureAwait(false);
}
