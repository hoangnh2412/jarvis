using Microsoft.Extensions.DependencyInjection;
using Platform.DDD.Domain.DataStorages;

namespace Platform.MultiOrg.DataStorages;

/// <summary>Domain org → <c>X-Org-Id</c> → query <c>orgId</c>.</summary>
public class OrgIdResolverFactory(IServiceProvider serviceProvider) : IOrgIdResolverFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public virtual async Task<Guid?> GetOrgIdAsync(CancellationToken cancellationToken = default)
    {
        foreach (var key in ResolverOrder)
        {
            var orgId = await TryResolveAsync(key, cancellationToken).ConfigureAwait(false);
            if (orgId.HasValue)
                return orgId;
        }

        return null;
    }

    internal static readonly string[] ResolverOrder =
    [
        nameof(OrgDomainIdResolver),
        nameof(HeaderOrgIdResolver),
        nameof(QueryOrgIdResolver),
    ];

    private async Task<Guid?> TryResolveAsync(string resolverKey, CancellationToken cancellationToken)
    {
        var resolver = _serviceProvider.GetRequiredKeyedService<IOrgIdResolver>(resolverKey);
        return await resolver.GetOrgIdAsync(cancellationToken).ConfigureAwait(false);
    }
}
