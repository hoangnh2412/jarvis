using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database.Poco;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class CityRepository : EntityRepository<City>, ICityRepository
    {
        public async Task<List<City>> QueryAllAsync()
        {
            return await Query.Query().ToListAsync();
        }
    }
}
