using Jarvis.Application.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Persistence.MultiTenancy;

/// <summary>
/// Use query string of request to tenant identification
/// </summary>
public class QueryTenantIdResolver : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public QueryTenantIdResolver(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        if (!Guid.TryParse(_httpContextAccessor.HttpContext.Request.Query["tenantId"].ToString(), out Guid tenantId))
            return Guid.Empty;

        return tenantId;
    }
}