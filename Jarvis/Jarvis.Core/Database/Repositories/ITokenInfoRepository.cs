using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Database.Repositories
{
    public interface ITokenInfoRepository : IRepository<TokenInfo>
    {
        /// <summary>
        /// lấy các token của các tài khoản trong 1 công ty/chi nhánh
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<List<TokenInfo>> QueryByTenantAsync(Guid tenantCode);

        Task<TokenInfo> QueryByCodeAsync(Guid code);

        /// <summary>
        /// lấy token theo các iduser
        /// </summary>
        /// <param name="idUsers"></param>
        /// <returns></returns>
        Task<List<TokenInfo>> QueryByUserAsync(List<Guid> idUsers);
    }
}
