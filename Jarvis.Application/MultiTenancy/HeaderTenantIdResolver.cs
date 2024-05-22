using Microsoft.AspNetCore.Http;

namespace Jarvis.Application.MultiTenancy;

/// <summary>
/// Use header of request to tenant identification
/// </summary>
public class HeaderTenantIdResolver : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderTenantIdResolver(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var id = _httpContextAccessor.HttpContext.Request.Headers["X-Tenant-Id"].ToString();
        if (id == null)
            return Guid.Empty;

        if (!Guid.TryParse(id, out Guid tenantId))
            return Guid.Empty;

        return tenantId;
    }
}