using Jarvis.Application.MultiTenancy;
using Jarvis.Domain.Common.Interfaces;
using Jarvis.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Persistence.MultiTenancy;

/// <summary>
/// Use query string of request to tenant identification
/// </summary>
public class QueryTenantIdentification : ITenantIdentification
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public QueryTenantIdentification(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<Guid> GetCurrentAsync()
    {
        if (!Guid.TryParse(_httpContextAccessor.HttpContext.Request.Query["tenantId"].ToString(), out Guid tenantId))
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