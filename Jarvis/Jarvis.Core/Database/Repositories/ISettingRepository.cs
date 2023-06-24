using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Database.Repositories
{
    public interface ISettingRepository : IRepository<Setting>
    {
        Task<List<Setting>> GetByTenantCodeAsync(Guid tenantCode);

        Task<List<Setting>> GetByTenantCodeAsync(string cacheKey, Guid tenantCode);

        Task<List<Setting>> GetByGroupAsync(Guid tenantCode, string group);

        Task<Setting> GetByKeyAsync(Guid tenantCode, string key);

        Task<Setting> GetByKeyAsync(string cacheKey, Guid tenantCode, string key);

        Task<Setting> GetByKeyAsync(string key);

        // Task InsertsAsync(List<Setting> entities);


        Task<List<Setting>> GetByTenantCodesAsync(List<Guid> tenantCodes);

        Task<List<Setting>> GetByGroupTenantAsync(Guid tenantCode, string group);
    }
}
