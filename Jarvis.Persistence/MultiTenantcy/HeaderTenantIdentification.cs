using Jarvis.Application.MultiTenancy;
using Jarvis.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Persistence.MultiTenancy;

/// <summary>
/// Use header of request to tenant identification
/// </summary>
public class HeaderTenantIdentification : ITenantIdentification
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderTenantIdentification(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<Guid> GetCurrentAsync()
    {
        var id = _httpContextAccessor.HttpContext.Request.Headers["X-Tenant-Id"].ToString();
        if (id == null)
            return Task.FromResult(Guid.Empty);

        if (!Guid.TryParse(id, out Guid tenantId))
            return Task.FromResult(Guid.Empty);

        return Task.FromResult(tenantId);
    }

    public Task<ITenant> GetTenantAsync(string hostname)
    {
        throw new NotImplementedException();
    }

    public Task<ITenant> GetTenantAsync(Guid tenantId)
    {
        throw new NotImplementedException();
    }
}