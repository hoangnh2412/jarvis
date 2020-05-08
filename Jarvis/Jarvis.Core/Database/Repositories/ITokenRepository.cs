using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Database.Repositories
{
    public interface ITokenRepository : IRepository<TokenInfo>
    {
        Task<TokenInfo> GetByCodeAsync(Guid code);

        Task<List<TokenInfo>> GetByUserAsync(Guid userCode);
        
        Task<TokenInfo> GetByUserAsync(Guid userCode, string userAgent, string localIpAddress, string remoteIpAddress);

        Task<List<TokenInfo>> GetUnexpiredTokenByUserAsync(Guid userCode);

        // void Deletes(List<TokenInfo> tokens);
    }
}
