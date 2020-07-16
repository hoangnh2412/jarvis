using Jarvis.Core.Database.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Jarvis.Core.Database;
using Infrastructure.Caching;

namespace Jarvis.Core.Middlewares
{
    public class AuthMiddlerware
    {
        private readonly ILogger<AuthMiddlerware> _logger;
        private readonly RequestDelegate _next;
        private readonly ICacheService _cache;

        public AuthMiddlerware(
            ILogger<AuthMiddlerware> logger,
            RequestDelegate next,
            ICacheService cache)
        {
            _logger = logger;
            _next = next;
            _cache = cache;

        }

        /// <summary>
        /// Kiểm tra token còn hạn hay ko
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="uow"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IConfiguration configuration, ICoreUnitOfWork uow)
        {
            var auth = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(auth))
            {
                await _next.Invoke(context);
                return;
            }

            auth = auth.Replace("Bearer ", "");
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.ReadToken(auth);

            //lấy token từ cache
            var bytes = await _cache.GetAsync($":TokenInfos:{token.Id}");

            if (bytes != null)
            {
                await _next.Invoke(context);
                return;
            }

            //lấy từ DB ra xem có dữ liệu không
            var repoTenant = uow.GetRepository<ITokenInfoRepository>();
            var tokenInfo = await repoTenant.QueryByCodeAsync(Guid.Parse(token.Id));
            if (tokenInfo != null && tokenInfo.AccessToken == auth)
            {
                //lưu vào cache
                var cacheOption = new DistributedCacheEntryOptions();
                cacheOption.AbsoluteExpiration = tokenInfo.ExpireAt;
                await _cache.SetAsync($":TokenInfos:{tokenInfo.Code}", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(tokenInfo)), cacheOption);

                await _next.Invoke(context);
                return;
            }

            context.Response.StatusCode = 401; //UnAuthorized
            return;
        }
    }
}
