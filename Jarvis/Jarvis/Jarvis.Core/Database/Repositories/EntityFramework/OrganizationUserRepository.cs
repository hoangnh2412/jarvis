using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database.Poco;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class OrganizationUserRepository : EntityRepository<OrganizationUser>, IOrganizationUserRepository
    {
        public async Task<List<OrganizationUser>> GetUsersByOrganizationAsync(Guid organizationCode)
        {
            return await Query.Query(x => x.OrganizationCode == organizationCode).ToListAsync();
        }
    }
}
