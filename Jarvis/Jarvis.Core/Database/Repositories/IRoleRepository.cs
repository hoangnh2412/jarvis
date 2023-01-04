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

        Task<Role> GetRoleByIdAsync(ContextModel context, Guid id);

        Task<Role> GetRoleByIdAsync(Guid tenantCode, Guid id);

        Task<Paged<Role>> PagingAsync(Guid tenantCode, Paging paging);
    }
}
