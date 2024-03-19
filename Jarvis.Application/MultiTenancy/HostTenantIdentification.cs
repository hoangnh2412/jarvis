using Microsoft.AspNetCore.Http;

namespace Jarvis.Application.MultiTenancy;

/// <summary>
/// Use host name to tenant identification
/// </summary>
public class HostTenantIdentification : ITenantIdentification
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HostTenantIdentification(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<Guid> GetCurrentAsync()
    {
        var host = _httpContextAccessor.HttpContext.Request.Host.Value;
        return Task.FromResult(Guid.Empty);
    }
}