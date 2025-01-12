using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.EntityFramework.DataStorages;

/// <summary>
/// Base class for DbContextFactory
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext, must implement BaseStorageContext</typeparam>
/// <typeparam name="TTenantIdResolver">The type of the TenantIdResolver, must implement ITenantIdResolver</typeparam>
/// <typeparam name="TTenantConnectionStringResolver">The type of the TenantConnectionStringResolver, must implement ITenantConnectionStringResolver</typeparam>
public class BaseDbContextFactory<TDbContext, TTenantIdResolver, TTenantConnectionStringResolver>(
    IServiceProvider serviceProvider,
    Action<DbContextOptionsBuilder, ITenantIdResolver, ITenantConnectionStringResolver> configureDbContext)
    : IDbContextFactory<TDbContext>
    where TDbContext : DbContext
    where TTenantIdResolver : ITenantIdResolver
    where TTenantConnectionStringResolver : ITenantConnectionStringResolver
{
    private readonly IServiceProvider ServiceProvider = serviceProvider;
    protected readonly Action<DbContextOptionsBuilder, ITenantIdResolver, ITenantConnectionStringResolver> ConfigureDbContext = configureDbContext;

    /// <summary>
    /// Create DbContext in a synchronous manner.
    /// </summary>
    /// <returns>The DbContext instance</returns>
    public virtual TDbContext CreateDbContext()
    {
        var dbContext = CreateDbContextInternal();
        var tenantIdResolver = ServiceProvider.GetRequiredKeyedService<ITenantIdResolver>(typeof(TTenantIdResolver).Name);

        var tenantId = tenantIdResolver.GetTenantId();
        if (tenantId != null)
            ((IStorageContext)dbContext).SetTenantId(tenantId);

        return dbContext;
    }

    private TDbContext CreateDbContextInternal()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        // ConfigureDbContext.Invoke(
        //     optionsBuilder,
        //     ServiceProvider.GetRequiredKeyedService<ITenantIdResolver>(typeof(TTenantIdResolver).Name),
        //     ServiceProvider.GetRequiredKeyedService<ITenantConnectionStringResolver>(typeof(TTenantConnectionStringResolver).Name));

        var tenantIdResolver = new HeaderTenantIdResolver(ServiceProvider.GetRequiredService<IHttpContextAccessor>(), "X-Tenant-Id");
        var tenantConnectionResolver = ServiceProvider.GetRequiredKeyedService<ITenantConnectionStringResolver>(typeof(TTenantConnectionStringResolver).Name);
        ConfigureDbContext.Invoke(optionsBuilder, tenantIdResolver, tenantConnectionResolver);


        var dbContext = Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
        return dbContext as TDbContext ?? throw new InvalidOperationException($"Can't cast {dbContext.GetType().Name} to {typeof(TDbContext).FullName}");
    }

    /// <summary>
    /// Create DbContext in a asynchronous manner.
    /// </summary>
    /// <returns>The DbContext instance</returns>
    public virtual async Task<TDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = CreateDbContextInternal();
        var tenantIdResolver = ServiceProvider.GetRequiredKeyedService<ITenantIdResolver>(typeof(TTenantIdResolver).Name);
        var tenantId = await tenantIdResolver.GetTenantIdAsync(cancellationToken);
        if (tenantId != null)
            ((IStorageContext)dbContext).SetTenantId(tenantId);
        return dbContext;
    }
}