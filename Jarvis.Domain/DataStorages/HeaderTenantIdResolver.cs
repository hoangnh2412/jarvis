using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Use header of request to tenant identification
/// </summary>
public class HeaderTenantIdResolver(
    IHttpContextAccessor httpContextAccessor)
    : ITenantIdResolver
{
    public static string HeaderTenantId = "X-Tenant-Id";

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid Resolve()
    {
        var id = _httpContextAccessor.HttpContext?.Request.Headers[HeaderTenantId].ToString();
        if (id == null)
            return Guid.Empty;

        if (!Guid.TryParse(id, out Guid tenantId))
            return Guid.Empty;

        return tenantId;
    }

    public Task<Guid> ResolveAsync()
    {
        return Task.FromResult(Resolve());
    }
}