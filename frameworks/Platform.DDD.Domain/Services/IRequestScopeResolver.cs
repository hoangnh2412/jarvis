namespace Platform.DDD.Domain.Services;

/// <summary>
/// Xác định working org + tenant theo header <c>X-Org-Id</c> / <c>X-Tenant-Id</c> và quy tắc platform.
/// </summary>
public interface IRequestScopeResolver
{
    Task<RequestScopeResolution> ResolveAsync(CancellationToken cancellationToken = default);
}

public readonly record struct RequestScopeResolution(Guid? OrgId, Guid? TenantId, bool IsForbidden)
{
    public static RequestScopeResolution Forbidden => new(null, null, true);

    public static RequestScopeResolution Allowed(Guid orgId, Guid tenantId) => new(orgId, tenantId, false);

    public static RequestScopeResolution Empty => new(null, null, false);

    public bool IsAllowed => !IsForbidden && OrgId is { } o && o != Guid.Empty && TenantId is { } t && t != Guid.Empty;

    public bool HasOrg => IsAllowed;
}
