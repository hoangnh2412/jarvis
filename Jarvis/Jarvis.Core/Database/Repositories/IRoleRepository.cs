using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Entities;
using Infrastructure.Database.Models;
using Jarvis.Core.Models;
using System;
using System.Threading.Tasks;

namespace Jarvis.Core.Database.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Paged<Role>> PagingAsync(ContextModel context, Paging paging);

        Task<Role> GetRoleByKeyAsync(ContextModel context, Guid key);

        Task<Role> GetRoleByKeyAsync(Guid tenantKey, Guid key);

        Task<Paged<Role>> PagingAsync(Guid tenantKey, Paging paging);
    }
}
