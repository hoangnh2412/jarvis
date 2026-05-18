using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.EntityFramework.DataStorages;

public class DbTenantConnectionStringResolver<TDbContext>(
    IDbContextFactory<TDbContext> dbContextFactory)
    : ITenantConnectionStringResolver<TDbContext>
    where TDbContext : DbContext, IStorageContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory = dbContextFactory;

    public virtual async Task<string?> GetConnectionStringAsync(string name, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(name, out var tenantId))
            return null;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var tenant = await dbContext.Set<ITenantManagementEntity>()
            .FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken)
            .ConfigureAwait(false);

        return tenant?.ConnectionString;
    }
}
