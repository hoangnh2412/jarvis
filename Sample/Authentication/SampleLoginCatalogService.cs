using Microsoft.EntityFrameworkCore;
using Platform.DDD.Domain.Repositories;
using Sample.Entities;
using Sample.Persistence;

namespace Sample.Authentication;

public sealed record LoginScopeOption(Guid Id, string Code, string Name);

/// <summary>
/// Danh mục tenant/org cho màn đăng nhập. Path nằm dưới <c>/api/authentication</c> nên không cần working org.
/// </summary>
public sealed class SampleLoginCatalogService(
    IMasterUnitOfWork master,
    ITenantUnitOfWork tenants)
{
    public async Task<IReadOnlyList<LoginScopeOption>> ListTenantsAsync(CancellationToken cancellationToken)
    {
        var repo = await master.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken).ConfigureAwait(false);
        var rows = await repo.GetQuery()
            .OrderBy(row => row.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Where(row => row.IsActive)
            .Select(row => new LoginScopeOption(row.Id, row.Code, string.IsNullOrWhiteSpace(row.Name) ? row.Code : row.Name))
            .ToArray();
    }

    public async Task<IReadOnlyList<LoginScopeOption>> ListOrgsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var masterRepo = await master.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken).ConfigureAwait(false);
        var known = await masterRepo.GetQuery()
            .AnyAsync(row => row.Id == tenantId, cancellationToken)
            .ConfigureAwait(false);
        if (!known)
            return [];

        await tenants.SwitchDbContextAsync(tenantId, cancellationToken).ConfigureAwait(false);
        var repo = await tenants.GetRepositoryAsync<IRepository<Sample.Entities.Org>>(cancellationToken).ConfigureAwait(false);
        var rows = await repo.GetQuery()
            .Where(row => row.IsActive)
            .OrderBy(row => row.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(row => new LoginScopeOption(row.Id, row.Code, row.Name))
            .ToArray();
    }
}
