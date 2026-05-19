using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Default <see cref="ITenantIdResolverFactory"/> implementation. Resolves the current tenant by trying
/// <see cref="ICurrentTenantAccessor"/> first, then keyed <see cref="ITenantIdResolver"/> services in order:
/// <see cref="HeaderTenantIdResolver"/> → <see cref="UserTenantIdResolver"/> → <see cref="QueryTenantIdResolver"/> →
/// <see cref="HostTenantIdResolver"/>.
/// </summary>
public class TenantIdResolverFactory(
    IServiceProvider serviceProvider,
    ICurrentTenantAccessor currentTenantAccessor)
    : ITenantIdResolverFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ICurrentTenantAccessor _currentTenantAccessor = currentTenantAccessor;

    public virtual async Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTenantAccessor.TenantId.HasValue)
            return _currentTenantAccessor.TenantId;

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
