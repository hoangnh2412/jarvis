using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database.Poco;
using Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Jarvis.Core.Permissions;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class TenantRepository : EntityRepository<Tenant>, ITenantRepository
    {
        public async Task<bool> AnyByNameAsync(string name)
        {
            IQueryable<Tenant> query = DbSet.Where(x => x.Name == name);
            query = query.QueryByDeletedBy();
            return await query.AnyAsync();
        }

        public async Task<bool> AnyByHostNameAsync(string hostName)
        {
            IQueryable<TenantHost> query = StorageContext.Set<TenantHost>();
            query = query.Where(x => x.HostName == hostName && !x.DeletedVersion.HasValue);

            return await query.AnyAsync();
        }

        public async Task<bool> AnyAsync()
        {
            return await Query.AnyAsync();
        }

        public async Task<Paged<Tenant>> PagingAsync(Guid idTenant, Paging paging)
        {
            if (string.IsNullOrEmpty(paging.Q))
            {
                var paged = await Query
                    .Query(
                        filter: queryable =>
                        {
                            queryable = queryable.QueryByDeletedBy();

                            queryable = queryable.Where(x => x.Path != null && x.Path.Contains(idTenant.ToString()) && !x.Path.EndsWith(idTenant.ToString()));

                            return queryable;
                        },
                        order: items => items.OrderByDescending(x => x.ExpireDate),
                        include: null)
                    .ToPaginationAsync(paging);

                return paged;
            }
            else
            {
                var codeTenants = await Query
                    .Query(
                        filter: queryable =>
                        {
                            queryable = queryable.QueryByDeletedBy();

                            queryable = queryable.Where(x => x.Path != null && x.Path.Contains(idTenant.ToString()) && !x.Path.EndsWith(idTenant.ToString()));

                            return queryable;
                        },
                        order: null,
                        include: null)
                    .Select(x => x.Code)
                    .AsQueryable()
                    .ToListAsync();

                //query tên miền
                IQueryable<TenantHost> queryTenantHost = StorageContext.Set<TenantHost>().Where(x => codeTenants.Contains(x.Code) && !x.DeletedVersion.HasValue);
                var codeTenantHosts = await queryTenantHost.Where(x => x.HostName.Contains(paging.Q)).Select(x => x.Code).AsQueryable().ToListAsync();

                //query các thông tin khác
                IQueryable<TenantInfo> queryTenantInfo = StorageContext.Set<TenantInfo>().Where(x => codeTenants.Contains(x.Code));
                var codeTenantInfos = await queryTenantInfo.Where(x => x.TaxCode.Contains(paging.Q)
                                                          || x.FullNameVi.Contains(paging.Q)
                                                          || x.Address.Contains(paging.Q))
                                                   .Select(x => x.Code).AsQueryable().ToListAsync();

                codeTenants = codeTenantHosts;
                codeTenants.AddRange(codeTenantInfos);

                codeTenants.Distinct();

                var paged = await Query
                   .Query(
                       filter: queryable =>
                       {
                           queryable = queryable.Where(x => codeTenants.Contains(x.Code));

                           return queryable;
                       },
                       order: items => items.OrderByDescending(x => x.ExpireDate),
                       include: null)
                   .ToPaginationAsync(paging);

                return paged;
            }
        }

        public async Task<List<TenantInfo>> GetInfoByCodesAsync(List<Guid> tenantCodes)
        {
            IQueryable<TenantInfo> query = StorageContext.Set<TenantInfo>();
            query = query.Where(x => tenantCodes.Contains(x.Code));
            return await query.ToListAsync();
        }

        public async Task<List<TenantHost>> GetHostByCodesAsync(List<Guid> tenantCodes)
        {
            IQueryable<TenantHost> query = StorageContext.Set<TenantHost>();
            return await query.Where(x => tenantCodes.Contains(x.Code) && !x.DeletedVersion.HasValue).ToListAsync();
        }

        public async Task<TenantInfo> GetInfoByCodeAsync(Guid tenantCode)
        {
            IQueryable<TenantInfo> query = StorageContext.Set<TenantInfo>();
            var info = await query.FirstOrDefaultAsync(x => x.Code == tenantCode);
            return info;
        }

        public async Task<List<TenantHost>> GetHostByCodeAsync(Guid tenantCode)
        {
            IQueryable<TenantHost> query = StorageContext.Set<TenantHost>();
            return await query.Where(x => x.Code == tenantCode && !x.DeletedVersion.HasValue).ToListAsync();
        }

        public async Task<TenantHost> GetByHostAsync(string host)
        {
            IQueryable<TenantHost> query = StorageContext.Set<TenantHost>();
            return await query.FirstOrDefaultAsync(x => x.HostName == host && !x.DeletedVersion.HasValue);
        }

        public async Task<List<Tenant>> GetByCodesAsync(List<Guid> tenantCodes)
        {
            IQueryable<Tenant> query = StorageContext.Set<Tenant>();
            query = query.Where(x => tenantCodes.Contains(x.Code));
            query = query.QueryByDeletedBy();

            return await query.AsQueryable().ToListAsync();
        }

        public async Task<Tenant> GetByCodeAsync(Guid code)
        {
            return await Query.FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<Tenant> GetByCodeAsync(Guid code, Guid parent)
        {
            return await Query.FirstOrDefaultAsync(x => x.Code == code && x.Path.Contains(parent.ToString()));
        }

        public async Task InsertInfoAsync(TenantInfo info)
        {
            var dbset = StorageContext.Set<TenantInfo>();
            await dbset.AddAsync(info);
        }

        public async Task InsertHostAsync(TenantHost host)
        {
            var dbset = StorageContext.Set<TenantHost>();
            await dbset.AddAsync(host);
        }

        public async Task InsertHostsAsync(List<TenantHost> hosts)
        {
            var dbset = StorageContext.Set<TenantHost>();
            await dbset.AddRangeAsync(hosts);
        }

        public void UpdateInfo(TenantInfo info)
        {
            var dbset = StorageContext.Set<TenantInfo>();
            dbset.Update(info);
        }

        public void UpdateInfoFields(TenantInfo user, params KeyValuePair<Expression<Func<TenantInfo, object>>, object>[] properties)
        {
            var entry = StorageContext.Entry(user);
            if (entry.State == EntityState.Detached)
                StorageContext.Attach(user);

            foreach (var property in properties)
            {
                entry.Property(property.Key).CurrentValue = property.Value;
                entry.Property(property.Key).IsModified = true;
            }
        }


        public void UpdateTenantHostFields(TenantHost tenantHost, params KeyValuePair<Expression<Func<TenantHost, object>>, object>[] properties)
        {
            var entry = StorageContext.Entry(tenantHost);
            if (entry.State == EntityState.Detached)
                StorageContext.Attach(tenantHost);

            foreach (var property in properties)
            {
                entry.Property(property.Key).CurrentValue = property.Value;
                entry.Property(property.Key).IsModified = true;
            }
        }

        public void UpdateTenantFields(Tenant user, params KeyValuePair<Expression<Func<Tenant, object>>, object>[] properties)
        {
            var entry = StorageContext.Entry(user);
            if (entry.State == EntityState.Detached)
                StorageContext.Attach(user);

            foreach (var property in properties)
            {
                entry.Property(property.Key).CurrentValue = property.Value;
                entry.Property(property.Key).IsModified = true;
            }
        }

        public async Task<List<Tenant>> GetHierarchyByCodeAsync(Guid code)
        {
            return await Query
                .Where(x => x.Path.Contains(code.ToString()) && !x.DeletedAt.HasValue)
                .Select(x => new Tenant
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Path = x.Path,
                    IdParent = x.IdParent,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public void DeleteHosts(List<TenantHost> hosts)
        {
            var dbset = StorageContext.Set<TenantHost>();
            dbset.RemoveRange(hosts);
        }

        public async Task<TenantHost> GetHostByHostNameAsync(string hostName)
        {
            IQueryable<TenantHost> query = StorageContext.Set<TenantHost>();
            return await query.Where(x => x.HostName == hostName && !x.DeletedVersion.HasValue).FirstOrDefaultAsync();
        }

        public async Task<List<Guid>> QueryAllCodeAsync()
        {
            IQueryable<Tenant> query = StorageContext.Set<Tenant>();
            query = query.QueryByDeletedBy();

            return await query.Select(x => x.Code).AsQueryable().ToListAsync();
        }

        public async Task<List<TenantHost>> QueryHostByHostNameAsync(string hostName)
        {
            IQueryable<TenantHost> query = StorageContext.Set<TenantHost>();
            return await query.Where(x => x.HostName == hostName && !x.DeletedVersion.HasValue).ToListAsync();
        }
    }
}
