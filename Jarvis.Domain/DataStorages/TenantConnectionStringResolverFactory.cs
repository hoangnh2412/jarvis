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

        ITenantConnectionStringResolver resolver = _serviceProvider.GetRequiredKeyedService<ITenantConnectionStringResolver>(nameof(ConfigConnectionStringResolver));
        var tenantId = await _tenantIdResolverFactory.GetTenantIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null)
            return await resolver.GetConnectionStringAsync(dbContextType.Name, cancellationToken).ConfigureAwait(false);

        resolver = _serviceProvider.GetRequiredKeyedService<ITenantConnectionStringResolver>(dbContextType.Name);
        return await resolver.GetConnectionStringAsync(tenantId.Value.ToString(), cancellationToken).ConfigureAwait(false);
    }
}
