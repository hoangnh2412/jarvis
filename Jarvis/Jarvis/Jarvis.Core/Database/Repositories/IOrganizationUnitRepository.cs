using System;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Models;
using Jarvis.Core.Models;
using Jarvis.Core.Database.Poco;
using System.Threading.Tasks;

namespace Jarvis.Core.Database.Repositories
{
    public interface IOrganizationUnitRepository : IRepository<OrganizationUnit>
    {
        Task<Paged<OrganizationUnit>> PagingAsync(ContextModel context, Paging paging);

        Task<OrganizationUnit> GetByCodeAsync(ContextModel context, Guid code);
    }
}
