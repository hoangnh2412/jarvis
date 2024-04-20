using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;
using Jarvis.Persistence.DataContexts;
using Jarvis.Persistence.MultiTenancy;

namespace Sample.DataStorage;

public class StorageConnectionStringResolver : ITenantConnectionStringResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StorageConnectionStringResolver(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetConnectionString(string tenantIdOrName = null)
    {
        Guid tenantId = GetTenantId(tenantIdOrName);

        var uow = _httpContextAccessor.HttpContext.RequestServices.GetService<ITenantUnitOfWork>();
        var repo = uow.GetRepository<IRepository<Tenant>>();
        var tenant = repo.GetQuery().FirstOrDefault(x => x.Id == tenantId);
        if (tenant == null)
            throw new Exception($"Not found connection string of tenant {tenantId}");

        return tenant.ConnectionString;
    }

    private Guid GetTenantId(string tenantIdOrName)
    {
        var tenantId = Guid.Empty;
        if (string.IsNullOrEmpty(tenantIdOrName))
        {
            var factory = _httpContextAccessor.HttpContext.RequestServices.GetService<Func<string, ITenantIdResolver>>();

            ITenantIdResolver resolver = null;
            if (!string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Request.Headers["X-Tenant-Id"].ToString()))
                resolver = factory.Invoke(typeof(HeaderTenantIdResolver).AssemblyQualifiedName);
            else if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("tenantId", out Microsoft.Extensions.Primitives.StringValues tenantIdString))
                resolver = factory.Invoke(typeof(QueryTenantIdResolver).AssemblyQualifiedName);
            else
                resolver = factory.Invoke(typeof(HostTenantIdResolver).AssemblyQualifiedName);

            tenantId = resolver.GetTenantId();
        }
        else
        {
            tenantId = Guid.Parse(tenantIdOrName);
        }

        return tenantId;
    }
}