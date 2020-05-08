using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database.Repositories.EntityFramework;
using Jarvis.Core.Database.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using Jarvis.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class TokenRepository : EntityRepository<TokenInfo>, ITokenRepository
    {
        public async Task<TokenInfo> GetByCodeAsync(Guid code)
        {
            return await Query.FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<List<TokenInfo>> GetByUserAsync(Guid userCode)
        {
            return await Query.Query(x => x.IdUser == userCode).ToListAsync();
        }

        public async Task<TokenInfo> GetByUserAsync(Guid userCode, string userAgent, string localIpAddress, string remoteIpAddress)
        {
            return await Query.FirstOrDefaultAsync(x => x.IdUser == userCode && x.UserAgent == userAgent && x.LocalIpAddress == localIpAddress && x.PublicIpAddress == remoteIpAddress);
        }

        public async Task<List<TokenInfo>> GetUnexpiredTokenByUserAsync(Guid userCode)
        {
            return await Query.Query(x => x.IdUser == userCode && x.ExpireAtUtc > DateTime.UtcNow).ToListAsync();
        }
    }
}
