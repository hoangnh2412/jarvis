using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Default <see cref="ITenantIdResolverFactory"/> implementation. Resolves the current tenant by trying keyed
/// <see cref="ITenantIdResolver"/> services in order: <see cref="HeaderTenantIdResolver"/> →
/// <see cref="UserTenantIdResolver"/> → <see cref="QueryTenantIdResolver"/> → <see cref="HostTenantIdResolver"/>.
/// Returns the first non-null <see cref="Guid"/>, or <c>null</c> if none match.
/// </summary>
public sealed class TenantIdResolverFactory(IServiceProvider serviceProvider) : ITenantIdResolverFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default)
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
