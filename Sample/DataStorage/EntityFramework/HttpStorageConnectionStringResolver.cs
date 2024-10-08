using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;
using Jarvis.Shared.DependencyInjection;

namespace Sample.DataStorage.EntityFramework;

public class HttpStorageConnectionStringResolver : ITenantConnectionStringResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpStorageConnectionStringResolver(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetConnectionString(string tenantIdOrName = null)
    {
        Guid tenantId = GetTenantId(tenantIdOrName);

        var uow = _httpContextAccessor.HttpContext.RequestServices.GetService<ITenantUnitOfWork>();
        var repo = uow.GetRepository<IEFRepository<Tenant>>();
        var tenant = repo.GetQuery().FirstOrDefault(x => x.Id == tenantId);
        if (tenant == null)
            throw new Exception($"Connection string of tenant {tenantId} not found");

        return tenant.ConnectionString;
    }

    private Guid GetTenantId(string tenantIdOrName)
    {
        var tenantId = Guid.Empty;
        if (string.IsNullOrEmpty(tenantIdOrName))
        {
            ITenantIdResolver resolver = null;
            if (!string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Request.Headers["X-Tenant-Id"].ToString()))
                resolver = _httpContextAccessor.HttpContext.RequestServices.GetService<ITenantIdResolver>(nameof(HeaderTenantIdResolver));
            else if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("tenantId", out Microsoft.Extensions.Primitives.StringValues tenantIdString))
                resolver = _httpContextAccessor.HttpContext.RequestServices.GetService<ITenantIdResolver>(nameof(QueryTenantIdResolver));
            else
                resolver = _httpContextAccessor.HttpContext.RequestServices.GetService<ITenantIdResolver>(nameof(HostTenantIdResolver));

            tenantId = resolver.GetTenantId();
        }
        else
        {
            tenantId = Guid.Parse(tenantIdOrName);
        }

        return tenantId;
    }
}