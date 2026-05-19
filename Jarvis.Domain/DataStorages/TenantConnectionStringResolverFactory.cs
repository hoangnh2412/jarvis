using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Default <see cref="ITenantConnectionStringResolverFactory"/>: tenant id from <see cref="ITenantIdResolverFactory"/>,
/// connection string from keyed <see cref="ITenantConnectionStringResolver"/> (key = <c>DbContext</c> type name):
/// no tenant → <c>ConnectionStrings:{DbContextName}</c>; with tenant → connection name = tenant <see cref="Guid"/>.
/// </summary>
public sealed class TenantConnectionStringResolverFactory(
    ITenantIdResolverFactory tenantIdResolverFactory,
    IServiceProvider serviceProvider)
    : ITenantConnectionStringResolverFactory
{
    private readonly ITenantIdResolverFactory _tenantIdResolverFactory = tenantIdResolverFactory;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<string?> GetConnectionStringAsync(Type dbContextType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContextType);


        var tenantId = await _tenantIdResolverFactory.GetTenantIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null)
        {
            var configResolver = _serviceProvider.GetRequiredKeyedService<ITenantConnectionStringResolver>(nameof(ConfigConnectionStringResolver));
            return await configResolver.GetConnectionStringAsync(dbContextType.Name, cancellationToken).ConfigureAwait(false);
        }

        var tenantResolver = _serviceProvider.GetRequiredKeyedService<ITenantConnectionStringResolver>(dbContextType.Name);
        var connectionString = await tenantResolver
            .GetConnectionStringAsync(tenantId.Value.ToString(), cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"No connection string resolved for tenant '{tenantId}' and DbContext '{dbContextType.Name}'. " +
                "Register the tenant in the master database or use AddCoreDbContext<TDb, TConnectionStringResolver> for dedicated databases.");
        }

        return connectionString;
    }
}
