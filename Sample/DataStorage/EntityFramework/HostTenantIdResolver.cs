using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;

namespace Sample.DataStorage.EntityFramework;

/// <summary>
/// Use host name to tenant identification
/// </summary>
public class HostTenantIdResolver : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HostTenantIdResolver(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var hostname = _httpContextAccessor.HttpContext.Request.Host.Value;
        var uow = _httpContextAccessor.HttpContext.RequestServices.GetService<ITenantUnitOfWork>();
        var repo = uow.GetRepository<IRepository<Tenant>>();
        var tenant = repo.GetQuery().FirstOrDefault(x => x.Name == hostname);
        if (tenant == null)
            return Guid.Empty;

        return tenant.Id;
    }
}