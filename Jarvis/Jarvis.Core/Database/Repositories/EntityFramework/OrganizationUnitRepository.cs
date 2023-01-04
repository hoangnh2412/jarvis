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
            return await query.FirstOrDefaultAsync(x => x.Code == code && !x.DeletedVersion.HasValue);
        }

        public async Task<OrganizationUnit> GetByCodeAsync(Guid tenantCode, Guid code)
        {
            return await Query.FirstOrDefaultAsync(x => x.Code == code && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue);
        }

        public async Task<List<OrganizationUnit>> GetByCodesAsync(Guid tenantCode, List<Guid> codes)
        {
            return await Query.Where(x => codes.Contains(x.Code) && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue).ToListAsync();
        }

        public async Task<List<OrganizationUnit>> GetNodeLeafsAsync(Guid tenantCode)
        {
            return await Query.Where(x => x.Right == x.Left + 1 && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue).ToListAsync();
        }

        public async Task<bool> IsNodeLeafAsync(Guid tenantCode, Guid code)
        {
            return await Query.AnyAsync(x => x.Right == x.Left + 1 && x.Code == code && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue);
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

        public async Task<int> ShiftRightNodeAsync(OrganizationUnit node, int space)
        {
            // var type = StorageContext.Model.FindEntityType(typeof(OrganizationUnit));
            // var schema = type.GetSchema();
            // var tableName = type.GetTableName();
            // var databaseName = StorageContext.Database.GetDbConnection().Database;

            // var entity = $"{databaseName}.";
            // if (!string.IsNullOrWhiteSpace(schema))
            // {
            //     entity += $"{schema}.";
            // }
            // entity += $"{tableName}";

            return await StorageContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE core_organization_unit SET `Right` = `Right` + {space} WHERE `Right` >= {node.Right}");
        }

        public async Task<int> ShiftLeftNodeAsync(OrganizationUnit node, int space)
        {
            // var type = StorageContext.Model.FindEntityType(typeof(OrganizationUnit));
            // var schema = type.GetSchema();
            // var tableName = type.GetTableName();
            // var databaseName = StorageContext.Database.GetDbConnection().Database;

            // var entity = $"`{databaseName}`.";
            // if (!string.IsNullOrWhiteSpace(schema))
            // {
            //     entity += $"`{schema}`.";
            // }
            // entity += tableName;

            return await StorageContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE core_organization_unit SET `Left` = `Left` + {space} WHERE `Left` >= {node.Right}");
        }

        public async Task<List<OrganizationUnit>> GetAllAsync(Guid tenantCode)
        {
            var units = await Query
                .Query(
                    filter: (queryable) =>
                    {
                        queryable = queryable.QueryByDeletedBy();
                        queryable = queryable.QueryByTenantCode(tenantCode);
                        return queryable;
                    },
                    order: x => x.OrderByDescending(y => y.CreatedAt),
                    include: null)
                .ToListAsync();
            return units;
        }

        public async Task<List<OrganizationUnit>> GetChildrenAsync(Guid tenantCode, OrganizationUnit node)
        {
            var children = await Query.Where(x => x.Left > node.Left && x.Right < node.Right && !x.DeletedVersion.HasValue).ToListAsync();
            return children;
        }
    }
}
