using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database.Poco;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class DistrictRepository : EntityRepository<Disctrict>, IDistrictRepository
    {
        public async Task<List<Disctrict>> QueryAllAsync()
        {
            return await Query.Query().ToListAsync();
        }
    }
}
