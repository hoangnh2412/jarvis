using Microsoft.AspNetCore.Http;
using Platform.DDD.Domain.DataStorages;

namespace Platform.MultiOrg.DataStorages;

/// <summary>Bước 1: domain request → org qua <see cref="IOrgDomainLookup"/>.</summary>
public sealed class OrgDomainIdResolver(
    IHttpContextAccessor httpContextAccessor,
    IOrgDomainLookup? orgDomainLookup = null) : IOrgIdResolver
{
    public async Task<Guid?> GetOrgIdAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (orgDomainLookup is null)
            return null;

        var host = httpContextAccessor.HttpContext?.Request.Host.Host;
        if (string.IsNullOrWhiteSpace(host))
            return null;

        return await orgDomainLookup.ResolveOrgIdByHostAsync(host, cancellationToken).ConfigureAwait(false);
    }
}
