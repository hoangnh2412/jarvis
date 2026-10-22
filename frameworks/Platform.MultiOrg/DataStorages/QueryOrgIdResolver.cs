using Microsoft.AspNetCore.Http;
using Platform.DDD.Domain.DataStorages;

namespace Platform.MultiOrg.DataStorages;

/// <summary>Bước 2 (sau header): query <c>orgId</c>.</summary>
public sealed class QueryOrgIdResolver(IHttpContextAccessor httpContextAccessor) : IOrgIdResolver
{
    public const string QueryName = "orgId";

    public Task<Guid?> GetOrgIdAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (httpContextAccessor.HttpContext == null)
            return Task.FromResult<Guid?>(null);

        var raw = httpContextAccessor.HttpContext.Request.Query[QueryName].ToString();
        if (string.IsNullOrWhiteSpace(raw))
            return Task.FromResult<Guid?>(null);

        return Task.FromResult(Guid.TryParse(raw, out var id) ? id : (Guid?)null);
    }
}
