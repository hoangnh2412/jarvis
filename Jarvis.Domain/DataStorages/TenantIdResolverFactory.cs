using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Default <see cref="ITenantIdResolverFactory"/>: keyed <see cref="ITenantIdResolver"/> in order —
/// <see cref="HeaderTenantIdResolver"/> → <see cref="UserTenantIdResolver"/> → <see cref="QueryTenantIdResolver"/> →
/// <see cref="HostTenantIdResolver"/>.
/// Does not read <see cref="ICurrentTenantAccessor"/> (UoW and HTTP-only resolution).
/// Connection opening reads the accessor first in <see cref="TenantConnectionStringResolverFactory"/>.
/// </summary>
public class TenantIdResolverFactory(IServiceProvider serviceProvider) : ITenantIdResolverFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public virtual async Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = await TryResolveAsync(nameof(HeaderTenantIdResolver), cancellationToken).ConfigureAwait(false);
        if (tenantId.HasValue)
            return tenantId;

        tenantId = await TryResolveAsync(nameof(UserTenantIdResolver), cancellationToken).ConfigureAwait(false);
        if (tenantId.HasValue)
            return tenantId;

        tenantId = await TryResolveAsync(nameof(QueryTenantIdResolver), cancellationToken).ConfigureAwait(false);
        if (tenantId.HasValue)
            return tenantId;

        tenantId = await TryResolveAsync(nameof(HostTenantIdResolver), cancellationToken).ConfigureAwait(false);
        if (tenantId.HasValue)
            return tenantId;

        return null;
    }

    private async Task<Guid?> TryResolveAsync(string resolverKey, CancellationToken cancellationToken)
    {
        var resolver = _serviceProvider.GetRequiredKeyedService<ITenantIdResolver>(resolverKey);
        return await resolver.GetTenantIdAsync(cancellationToken).ConfigureAwait(false);
    }
}
