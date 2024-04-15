using Jarvis.Application.MultiTenancy;
using Jarvis.Persistence.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Sample.DataStorage;

public class TenantIdAccessor : ITenantIdAccessor
{
    private readonly Func<string, ITenantIdentification> _identificationFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantIdAccessor(
        Func<string, ITenantIdentification> identificationFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _identificationFactory = identificationFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid> GetCurrentAsync()
    {
        ITenantIdentification identification = null;
        if (!string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Request.Headers["X-Tenant-Id"].ToString()))
            identification = _identificationFactory.Invoke(typeof(HeaderTenantIdentification).AssemblyQualifiedName);
        else if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("tenantId", out Microsoft.Extensions.Primitives.StringValues tenantId))
            identification = _identificationFactory.Invoke(typeof(QueryTenantIdentification).AssemblyQualifiedName);
        else
            identification = _identificationFactory.Invoke(typeof(HostTenantIdentification).AssemblyQualifiedName);

        return await identification.GetCurrentAsync();
    }
}