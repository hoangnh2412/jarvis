using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Database.Repositories
{
    public interface IDistrictRepository : IRepository<Disctrict>
    {
        Task<List<Disctrict>> QueryAllAsync();
    }
}
