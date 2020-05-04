using Infrastructure.Database.Abstractions;
using Jarvis.Core.Database.Poco;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jarvis.Core.Database.Repositories
{
    public interface ICityRepository : IRepository<City>
    {
        Task<List<City>> QueryAllAsync();
    }
}
