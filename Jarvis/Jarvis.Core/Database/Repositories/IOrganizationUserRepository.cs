using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Models;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Database.Repositories
{
    public interface IOrganizationUserRepository : IRepository<OrganizationUser>
    {
        Task<Paged<OrganizationUser>> PagingAsync(Guid organizationCode, Paging paging);
        
        Task<List<OrganizationUser>> GetUsersByOrganizationAsync(Guid organizationCode);

        // Task InsertsAsync(List<OrganizationUser> entities);

        // void Deletes(List<OrganizationUser> entities);
    }
}
