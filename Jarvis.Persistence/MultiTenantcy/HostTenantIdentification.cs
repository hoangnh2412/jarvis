using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;
using Jarvis.Domain.Entities;
using Jarvis.Persistence.DataContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Persistence.MultiTenancy;

/// <summary>
/// Use host name to tenant identification
/// </summary>
public class HostTenantIdentification : ITenantIdentification
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICoreUnitOfWork _uow;

    public HostTenantIdentification(
        IHttpContextAccessor httpContextAccessor,
        ICoreUnitOfWork uow)
    {
        _httpContextAccessor = httpContextAccessor;
        _uow = uow;
    }

    public async Task<Guid> GetCurrentAsync()
    {
        var hostname = _httpContextAccessor.HttpContext.Request.Host.Value;
        var entity = await GetTenantAsync(hostname);
        if (entity == null)
            return Guid.Empty;

        return entity.Id;
    }

    public async Task<ITenant> GetTenantAsync(string hostname)
    {
        var repo = _uow.GetRepository<IRepository<ITenant>>();
        return await repo.GetQuery().FirstOrDefaultAsync(x => x.Name == hostname);
    }

    public async Task<ITenant> GetTenantAsync(Guid tenantId)
    {
        var repo = _uow.GetRepository<IRepository<ITenant>>();
        return await repo.GetQuery().FirstOrDefaultAsync(x => x.Id == tenantId);
    }
}