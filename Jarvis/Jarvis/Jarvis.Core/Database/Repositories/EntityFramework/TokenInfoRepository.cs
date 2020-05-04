using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database.Poco;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class TokenInfoRepository : EntityRepository<TokenInfo>, ITokenInfoRepository
    {
        public async Task<List<TokenInfo>> QueryByTenantAsync(Guid tenantCode)
        {
            return await Query.Query(x => x.TenantCode == tenantCode)
                              .ToListAsync();
        }

        public async Task<TokenInfo> QueryByCodeAsync(Guid code)
        {
            return await Query.FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<List<TokenInfo>> QueryByUserAsync(List<Guid> idUsers)
        {
            return await Query.Query(x => idUsers.Contains(x.IdUser))
                              .ToListAsync();
        }
    }
}
