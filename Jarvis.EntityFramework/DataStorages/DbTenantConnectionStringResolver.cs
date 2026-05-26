using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.EntityFramework.DataStorages;

/// <summary>
/// Resolves a tenant connection string by loading <typeparamref name="TTenantEntity"/> from the master <typeparamref name="TMasterDbContext"/>.
/// </summary>
/// <typeparam name="TMasterDbContext">Master database context (e.g. stores tenants).</typeparam>
/// <typeparam name="TTenantEntity">Concrete entity type mapped in <typeparamref name="TMasterDbContext"/> (implements <see cref="ITenantManagementEntity"/>).</typeparam>
public class DbTenantConnectionStringResolver<TMasterDbContext, TTenantEntity>(
    IDbContextFactory<TMasterDbContext> dbContextFactory)
    : ITenantConnectionStringResolver
    where TMasterDbContext : DbContext, IStorageContext
    where TTenantEntity : class, ITenantManagementEntity
{
    private readonly IDbContextFactory<TMasterDbContext> _dbContextFactory = dbContextFactory;

    public virtual async Task<string?> GetConnectionStringAsync(string name, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(name, out var tenantId))
            return null;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var tenant = await dbContext.Set<TTenantEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken)
            .ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(tenant?.ConnectionString) ? null : tenant.ConnectionString;
    }
}
