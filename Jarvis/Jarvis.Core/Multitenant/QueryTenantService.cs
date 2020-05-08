using Jarvis.Core.Database.Poco;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using Newtonsoft.Json;
using Jarvis.Core.Database.Repositories;
using System.Threading.Tasks;
using Jarvis.Core.Database;

namespace Jarvis.Core.Multitenant
{
    public class QueryTenantService : ITenantIdentificationService
    {
        private readonly IDistributedCache _cache;
        private readonly ICoreUnitOfWork _uow;

        public QueryTenantService(
            IDistributedCache cache,
            ICoreUnitOfWork uow)
        {
            _cache = cache;
            _uow = uow;
        }

        public Task<List<Tenant>> GetChildrenTenantAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public async Task<Tenant> GetCurrentTenantAsync(HttpContext context)
        {
            if (!Guid.TryParse(context.Request.Query["tenantCode"].ToString(), out Guid tenantCode))
                return null;

            Tenant tenant;
            var cacheKey = $"TenantCode:{tenantCode}";

            //Lấy dữ liệu từ cache
            var bytes = await _cache.GetAsync(cacheKey);
            if (bytes != null)
            {
                var str = Encoding.UTF8.GetString(bytes);
                return JsonConvert.DeserializeObject<Tenant>(str);
            }

            //Lấy dữ liệu từ DB
            var host = context.Request.Host.Value;
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var current = await repoTenant.GetByHostAsync(host);
            if (current == null)
                return null;

            tenant = await repoTenant.GetByCodeAsync(tenantCode, current.Code);
            if (tenant == null)
                return null;

            //Cập nhật cache
            await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(tenant)));
            return tenant;
        }

        public Task<Tenant> GetParentTenantAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
