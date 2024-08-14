using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Use query string of request to tenant identification
/// </summary>
public class QueryTenantIdResolver(
    IHttpContextAccessor httpContextAccessor)
    : ITenantIdResolver
{
    public static string QueryTenantId = "tenantId";
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid Resolve()
    {
        if (!Guid.TryParse(_httpContextAccessor.HttpContext?.Request.Query[QueryTenantId].ToString(), out Guid tenantId))
            return Guid.Empty;

        return tenantId;
    }

    public Task<Guid> ResolveAsync()
    {
        return Task.FromResult(Resolve());
    }
}