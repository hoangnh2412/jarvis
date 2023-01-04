using Infrastructure.Database.Entities;
using Infrastructure.Database.EntityFramework;
using Infrastructure.Database.Models;
using Jarvis.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Jarvis.Core.Permissions;


namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class RoleRepository : EntityRepository<Role>, IRoleRepository
    {
        public async Task<Role> GetRoleByIdAsync(ContextModel context, Guid id)
        {
            IQueryable<Role> query = DbSet.Where(x => x.Id == id);
            //query = query.QueryByPermission(context);
            query = query.QueryByTenantCode(context.TenantCode);
            return await query.Take(1).AsQueryable().FirstOrDefaultAsync();
        }

        public async Task<Role> GetRoleByIdAsync(Guid tenantCode, Guid id)
        {
            IQueryable<Role> query = DbSet.Where(x => x.Id == id);
            query = query.QueryByTenantCode(tenantCode);
            return await query.Take(1).AsQueryable().FirstOrDefaultAsync();
        }

        public async Task<Paged<Role>> PagingAsync(ContextModel context, Paging paging)
        {
            var paged = await Query
                .Query(
                    filter: (items) =>
                    {
                        //items = items.QueryByPermission(context);
                        items = items.QueryByTenantCode(context.TenantCode);
                        items = items.QueryByDeletedBy();

                        if (!string.IsNullOrEmpty(paging.Q))
                        {
                            var q = paging.Q.ToUpper();
                            items = items.Where(x => x.NormalizedName.Contains(q));
                        }

                        if (paging.Search != null)
                        {
                            foreach (var item in paging.Search)
                            {
                                items = items.Contains(item.Key, item.Value);
                            }
                        }

                        if (paging.Sort != null)
                        {
                            items = items.OrderBy(paging.Sort);
                        }

                        if (paging.Columns != null)
                        {
                            items = items.Select(paging.Columns
                                .Where(x => x.Value)
                                .Select(x => x.Key)
                                .ToArray());
                        }

                        return items;
                    },
                    order: x => x.OrderByDescending(y => y.CreatedAt),
                    include: null)
                .ToPaginationAsync(paging);
            return paged;
        }

        public async Task<Paged<Role>> PagingAsync(Guid tenantCode, Paging paging)
        {
            var paged = await Query
                .Query(
                    filter: (items) =>
                    {
                        items = items.QueryByTenantCode(tenantCode);
                        items = items.QueryByDeletedBy();

                        return items;
                    },
                    order: x => x.OrderByDescending(y => y.CreatedAt),
                    include: null)
                .ToPaginationAsync(paging);
            return paged;
        }
    }
}
