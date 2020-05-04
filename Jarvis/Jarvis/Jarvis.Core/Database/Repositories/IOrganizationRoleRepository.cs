using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Database.Repositories
{
    public interface IOrganizationRoleRepository : IRepository<OrganizationRole>
    {
        IEnumerable<OrganizationRole> GetRolesByOrganization(Guid organizationCode);

        Task InsertsAsync(List<OrganizationRole> entities);

        void Deletes(List<OrganizationRole> entities);
    }
}
