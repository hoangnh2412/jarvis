using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database.Poco;
using Infrastructure.Database.Models;
using System;
using System.Linq;
using Jarvis.Core.Models;
using Jarvis.Core.Permissions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class OrganizationUnitRepository : EntityRepository<OrganizationUnit>, IOrganizationUnitRepository
    {
        public async Task<Paged<OrganizationUnit>> PagingAsync(Guid tenantCode, Guid userCode, Paging paging)
        {
            var paged = await Query
                .Query(
                    filter: (queryable) =>
                    {
                        queryable = queryable.QueryByDeletedBy();
                        queryable = queryable.QueryByTenantCode(tenantCode);
                        return queryable;
                    },
                    order: x => x.OrderByDescending(y => y.CreatedAt),
                    include: null)
                .ToPaginationAsync(paging);
            return paged;
        }

        public async Task<OrganizationUnit> GetByCodeAsync(ContextModel context, Guid code)
        {
            IQueryable<OrganizationUnit> query = Query.QueryByPermission(context);
            return await query.FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<List<OrganizationUnit>> GetByCodesAsync(Guid tenantCode, List<Guid> codes)
        {
            return await Query.Where(x => codes.Contains(x.Code) && x.TenantCode == tenantCode).ToListAsync();
        }

        public async Task<List<OrganizationUnit>> GetNodeLeafsAsync(Guid tenantCode)
        {
            return await Query.Where(x => x.Right == x.Left + 1 && x.TenantCode == tenantCode).ToListAsync();
        }

        public async Task<bool> IsNodeLeafAsync(Guid tenantCode, Guid code)
        {
            return await Query.AnyAsync(x => x.Right == x.Left + 1 && x.Code == code && x.TenantCode == tenantCode);
        }

        public Task<Dictionary<Guid, int>> GetNodeDepth(Guid tenantCode)
        {
            return null;
        }

        public Task AddNodeLeaf(OrganizationUnit nodeRight, OrganizationUnit node)
        {
            // StorageContext.Database.ExecuteSqlRawAsync()
            return null;
        }

        public Task ShiftNode(Guid node)
        {
            throw new NotImplementedException();
        }
    }
}
