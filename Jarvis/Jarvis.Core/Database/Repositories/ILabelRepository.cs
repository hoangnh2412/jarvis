using System;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.EntityFramework;
using Infrastructure.Database.Models;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Models;

namespace Jarvis.Core.Database.Repositories
{
    public interface ILabelRepository : IRepository<Label>
    {
        Task<Paged<Label>> PagingAsync(ContextModel context, Paging paging);

        Task<Label> GetByCodeAsync(ContextModel context, Guid code);

        Task<Label> GetByCodeAsync(Guid tenantCode, Guid code);
    }
}
