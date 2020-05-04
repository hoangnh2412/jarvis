using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Models;
using Jarvis.Core.Database.Poco;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Jarvis.Core.Database.Repositories
{
    public interface ITenantRepository : IRepository<Tenant>
    {
        Task<bool> AnyByNameAsync(string name);

        Task<bool> AnyByHostNameAsync(string hostName);

        Task<bool> AnyAsync();

        Task<Paged<Tenant>> PagingAsync(Guid tenantCode, Paging paging);

        Task<List<TenantInfo>> GetInfoByCodesAsync(List<Guid> tenantCodes);

        Task<List<TenantHost>> GetHostByCodesAsync(List<Guid> tenantCodes);

        Task<List<Tenant>> GetByCodesAsync(List<Guid> tenantCodes);

        Task<TenantInfo> GetInfoByCodeAsync(Guid tenantCode);

        Task<List<TenantHost>> GetHostByCodeAsync(Guid tenantCode);

        Task<TenantHost> GetByHostAsync(string host);

        Task<Tenant> GetByCodeAsync(Guid code);

        Task<Tenant> GetByCodeAsync(Guid code, Guid parent);

        Task<List<Tenant>> GetHierarchyByCodeAsync(Guid code);

        Task InsertInfoAsync(TenantInfo info);

        Task InsertHostAsync(TenantHost host);

        Task InsertHostsAsync(List<TenantHost> hosts);

        void UpdateInfo(TenantInfo info);

        void UpdateInfoFields(TenantInfo user, params KeyValuePair<Expression<Func<TenantInfo, object>>, object>[] properties);

        void UpdateTenantFields(Tenant user, params KeyValuePair<Expression<Func<Tenant, object>>, object>[] properties);
        void UpdateTenantHostFields(TenantHost user, params KeyValuePair<Expression<Func<TenantHost, object>>, object>[] properties);

        void DeleteHosts(List<TenantHost> hosts);
        Task<TenantHost> GetHostByHostNameAsync(string hostName);

        Task<List<TenantHost>> QueryHostByHostNameAsync(string hostName);

        /// <summary>
        /// lấy tất cả tenantCode
        /// </summary>
        /// <returns></returns>
        Task<List<Guid>> QueryAllCodeAsync();
    }
}
