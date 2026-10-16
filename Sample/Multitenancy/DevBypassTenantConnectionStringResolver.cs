using Microsoft.Extensions.Configuration;
using Platform.DDD.Domain.DataStorages;

namespace Sample.Multitenancy;

/// <summary>
/// TEMP: bypass Master.Tenant lookup — always use <c>ConnectionStrings:TenantDbContext</c>.
/// Re-enable <see cref="Platform.EntityFramework.DataStorages.DbTenantConnectionStringResolver{TMasterDbContext, TTenantEntity}"/>
/// when tenants are registered in SampleMaster.
/// </summary>
public sealed class DevBypassTenantConnectionStringResolver(IConfiguration configuration)
    : ITenantConnectionStringResolver
{
    private readonly IConfiguration _configuration = configuration;

    public Task<string?> GetConnectionStringAsync(string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // TEMP bypass — ignore tenant id from X-Tenant-Id; use shared tenant DB from config.
        var connectionString = _configuration.GetConnectionString("TenantDbContext");
        return Task.FromResult(string.IsNullOrWhiteSpace(connectionString) ? null : connectionString);

        // Original flow (DbTenantConnectionStringResolver): lookup SampleMaster.Tenant by Id
        // if (!Guid.TryParse(name, out var tenantId))
        //     return Task.FromResult<string?>(null);
        //
        // await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        // var tenant = await dbContext.Set<TTenantEntity>()
        //     .AsNoTracking()
        //     .FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken)
        //     .ConfigureAwait(false);
        //
        // return Task.FromResult(string.IsNullOrWhiteSpace(tenant?.ConnectionString) ? null : tenant.ConnectionString);
    }
}
