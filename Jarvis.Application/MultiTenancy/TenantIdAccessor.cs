using Microsoft.AspNetCore.Http;

namespace Jarvis.Application.MultiTenancy;

public class TenantIdAccessor : ITenantIdAccessor
{
    private readonly IEnumerable<ITenantIdentification> _identifications;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantIdAccessor(
        IEnumerable<ITenantIdentification> identifications,
        IHttpContextAccessor httpContextAccessor)
    {
        _identifications = identifications;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid> GetCurrentAsync()
    {
        ITenantIdentification identification = null;
        if (!string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Request.Headers["X-Tenant-Id"].ToString()))
            identification = _identifications.FirstOrDefault(x => x.GetType().Name == typeof(HeaderTenantIdentification).Name);
        else
            identification = _identifications.FirstOrDefault(x => x.GetType().Name == typeof(QueryTenantIdentification).Name);

        return await identification.GetCurrentAsync();
    }
}