using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using Newtonsoft.Json;
using Jarvis.Core.Database.Repositories;
using System.Threading.Tasks;
using Infrastructure.Caching;

namespace Jarvis.Core.Multitenant
{
    public class HostTenantService : ITenantIdentificationService
    {
        private readonly ICacheService _cache;
        private readonly ICoreUnitOfWork _uow;

        public HostTenantService(
            ICacheService cache,
            ICoreUnitOfWork uow)
        {
            _cache = cache;
            _uow = uow;
        }

        public async Task<Tenant> GetCurrentTenantAsync(HttpContext context)
        {
            var host = context.Request.Host.Value;

            Tenant tenant;
            var cacheKey = $":TenantHost:{host}";

            //Lấy dữ liệu từ cache
            var bytes = await _cache.GetAsync(cacheKey);
            if (bytes != null)
            {
                var str = Encoding.UTF8.GetString(bytes);
                tenant = JsonConvert.DeserializeObject<Tenant>(str);
                return tenant;
            }

            //Lấy dữ liệu từ DB
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var tenantHost = await repoTenant.GetByHostAsync(host);
            if (tenantHost == null)
                return null;

            tenant = await repoTenant.GetByCodeAsync(tenantHost.Key);

            //Cập nhật cache
            await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(tenant)));
            return tenant;
        }

        public async Task<Tenant> GetParentTenantAsync(HttpContext context)
        {
            var current = await GetCurrentTenantAsync(context);
            if (current == null)
                return null;

            if (current.IdParent == null)
                return null;

            var repoTenant = _uow.GetRepository<ITenantRepository>();
            return await repoTenant.GetByCodeAsync(current.IdParent.Value);
        }

        public async Task<List<Tenant>> GetChildrenTenantAsync(HttpContext context)
        {
            var current = await GetCurrentTenantAsync(context);
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var ids = current.Path.Split(';').Select(x => Guid.Parse(x)).ToList();
            return await repoTenant.GetByCodesAsync(ids);
        }
    }
}
