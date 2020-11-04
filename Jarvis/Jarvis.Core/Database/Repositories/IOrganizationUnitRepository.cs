using System;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Models;
using Jarvis.Core.Models;
using Jarvis.Core.Database.Poco;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Jarvis.Core.Database.Repositories
{
    public interface IOrganizationUnitRepository : IRepository<OrganizationUnit>
    {
        Task<Paged<OrganizationUnit>> PagingAsync(Guid tenantCode, Guid userCode, Paging paging);

        Task<OrganizationUnit> GetByCodeAsync(ContextModel context, Guid code);

        Task<List<OrganizationUnit>> GetByCodesAsync(Guid tenantCode, List<Guid> codes);

        Task<List<OrganizationUnit>> GetNodeLeafsAsync(Guid tenantCode);

        Task<bool> IsNodeLeafAsync(Guid tenantCode, Guid code);

        Task<Dictionary<Guid, int>> GetNodeDepth(Guid tenantCode);

        Task AddNodeLeaf(OrganizationUnit nodeRight, OrganizationUnit node);

        Task ShiftNode(Guid node);
    }
}
