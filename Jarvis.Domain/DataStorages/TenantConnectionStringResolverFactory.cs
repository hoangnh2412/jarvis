using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Default <see cref="ITenantConnectionStringResolverFactory"/>: tenant id from <see cref="ITenantIdResolverFactory"/>,
/// connection string from keyed <see cref="ITenantConnectionStringResolver"/> (see <c>TenantDbConnectionOptions</c>).
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

        var connectionName = tenantId?.ToString();
        if (string.IsNullOrEmpty(connectionName))
            connectionName = dbContextType.Name;

        var resolver = _serviceProvider.GetRequiredKeyedService<ITenantConnectionStringResolver>(dbContextType.Name);

        var connectionString = await resolver.GetConnectionStringAsync(connectionName, cancellationToken)
            .ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(connectionString) ? null : connectionString;
    }
}
