using Infrastructure.Database.EntityFramework;
using Infrastructure.Database.Models;
using Jarvis.Core.Database.Poco;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class OrganizationUserRepository : EntityRepository<OrganizationUser>, IOrganizationUserRepository
    {
        public async Task<Paged<OrganizationUser>> PagingAsync(Guid organizationCode, Paging paging)
        {
            return await Query.Query(x => x.OrganizationCode == organizationCode)
                .ToPaginationAsync(paging);
        }

        public async Task<List<OrganizationUser>> GetUsersByOrganizationAsync(Guid organizationCode)
        {
            return await Query.Query(x => x.OrganizationCode == organizationCode).ToListAsync();
        }

        public async Task<List<OrganizationUser>> GetOrganizationByUserAsync(Guid userCode)
        {
            return await Query.Where(x => x.IdUser == userCode).ToListAsync();
        }

        public async Task<List<OrganizationUser>> GetUsersByOrganizationsAsync(List<Guid> organizationCodes)
        {
            return await Query.Where(x => organizationCodes.Contains(x.OrganizationCode)).ToListAsync();
        }
    }
}
