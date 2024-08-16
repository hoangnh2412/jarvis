using Jarvis.Domain.DataStorages;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.EntityFramework.DataStorages;

/// <summary>
/// Base class for DbContextFactory
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext, must implement BaseStorageContext</typeparam>
public class BaseDbContextFactory<TDbContext> : IDbContextFactory<TDbContext> where TDbContext : BaseStorageContext<TDbContext>
{
    protected readonly ITenantIdResolver _tenantIdResolver;
    protected readonly IDbContextFactory<TDbContext> _dbContextFactory;
    protected Guid OrgId { get; set; }

    public BaseDbContextFactory(
        ITenantIdResolver tenantIdResolver,
        IDbContextFactory<TDbContext> dbContextFactory)
    {
        _tenantIdResolver = tenantIdResolver;
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Create DbContext in a synchronous manner.
    /// Use <see cref="CreateDbContextAsync(CancellationToken)"/> if possible
    /// </summary>
    /// <returns>The DbContext instance</returns>
    public virtual TDbContext CreateDbContext()
    {
        var dbContext = _dbContextFactory.CreateDbContext();
        var tenantKey = _tenantIdResolver.Resolve();
        dbContext.SetTenantId(tenantKey);
        return dbContext;
    }


    /// <summary>
    /// Create DbContext in a asynchronous manner.
    /// </summary>
    /// <returns>The DbContext instance</returns>
    public virtual async Task<TDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var tenantKey = await _tenantIdResolver.ResolveAsync();
        dbContext.SetTenantId(tenantKey);
        return dbContext;
    }
}