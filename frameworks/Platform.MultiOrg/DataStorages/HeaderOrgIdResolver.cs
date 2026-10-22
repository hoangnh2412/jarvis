using Microsoft.AspNetCore.Http;
using Platform.DDD.Domain.DataStorages;

namespace Platform.MultiOrg.DataStorages;

/// <summary>Working org chỉ qua HTTP header <c>X-Org-Id</c>.</summary>
public class HeaderOrgIdResolver(IHttpContextAccessor httpContextAccessor) : IOrgIdResolver
{
    public const string HeaderName = "X-Org-Id";

    public Task<Guid?> GetOrgIdAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (httpContextAccessor.HttpContext == null)
            return Task.FromResult<Guid?>(null);

        var raw = httpContextAccessor.HttpContext.Request.Headers[HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(raw))
            return Task.FromResult<Guid?>(null);

        return Task.FromResult(Guid.TryParse(raw, out var id) ? id : (Guid?)null);
    }
}
