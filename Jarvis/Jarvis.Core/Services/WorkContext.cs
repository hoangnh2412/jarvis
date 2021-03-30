﻿using Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Models;
using Jarvis.Core.Permissions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Constants;
using Jarvis.Core.Multitenant;
using Jarvis.Core.Database;

namespace Jarvis.Core.Services
{
    public interface IWorkContext
    {
        Guid? GetTokenCode();

        Guid GetUserCode();

        Task<User> GetUserAsync();

        /// <summary>
        /// Lấy sesstion theo token
        /// </summary>
        /// <returns></returns>
        Task<SessionModel> GetSessionAsync();

        /// <summary>
        /// Lấy TenantCode theo domain hoặc querystring
        /// </summary>
        /// <returns></returns>
        Task<Guid> GetTenantCodeAsync();

        /// <summary>
        /// Lấy Tenant theo domain hoặc querystring
        /// </summary>
        /// <returns></returns>
        Task<Tenant> GetCurrentTenantAsync();

        /// <summary>
        /// Lấy TenantCode trong token
        /// </summary>
        /// <returns></returns>
        Guid GetTenantCode();

        Task<bool> HasClaimsAsync(List<string> claims);

        Task<List<string>> GetClaimsAsync(string prefix);

        Task<KeyValuePair<ClaimOfResource, ClaimOfChildResource>> GetClaimOfResourceAsync(string claim);

        Task<ContextModel> GetContextAsync(string policy);

        Task<T> GetOrAddCachePerRequestAsync<T>(string key, Func<Task<T>> builder);

        string GetConnectionId();
    }


    public class WorkContext : IWorkContext
    {
        private User _currentUser;
        private SessionModel _currentToken;

        private readonly ICoreUnitOfWork _uow;
        private readonly HttpContext _httpContext;
        private readonly IEnumerable<ITenantIdentificationService> _identificationServices;
        private readonly UserManager<User> _userManager;
        private readonly IDistributedCache _cache;

        public WorkContext(
            ICoreUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<ITenantIdentificationService> identificationServices,
            UserManager<User> userManager,
            IDistributedCache cache)
        {
            _uow = uow;
            _httpContext = httpContextAccessor.HttpContext;
            _identificationServices = identificationServices;
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<User> GetUserAsync()
        {
            if (_currentUser != null)
                return _currentUser;

            //Nếu sử dụng JWT để đăng nhập
            if (_httpContext.User.Identity.AuthenticationType == "AuthenticationTypes.Federation")
            {
                var idUser = GetUserCode();
                if (idUser != Guid.Empty)
                    _currentUser = await _userManager.FindByIdAsync(idUser.ToString());
            }
            else //Lấy người dùng hiện tại theo Cookie
            {
                _currentUser = await _userManager.GetUserAsync(_httpContext.User);
            }

            return _currentUser;
        }

        public async Task<SessionModel> GetSessionAsync()
        {
            if (_currentToken != null)
            {
                return _currentToken;
            }

            var tokenCode = GetTokenCode();
            if (tokenCode == null)
                return null;

            var bytes = await _cache.GetAsync($":TokenInfos:{tokenCode}");
            if (bytes != null)
            {
                var data = JsonConvert.DeserializeObject<TokenInfo>(Encoding.UTF8.GetString(bytes));
                _currentToken = JsonConvert.DeserializeObject<SessionModel>(data.Metadata);
                return _currentToken;
            }

            var repoToken = _uow.GetRepository<ITokenRepository>();
            var token = await repoToken.GetByCodeAsync(tokenCode.Value);

            if (token == null)
                return null;

            _currentToken = JsonConvert.DeserializeObject<SessionModel>(token.Metadata);

            var cacheOption = new DistributedCacheEntryOptions();
            cacheOption.AbsoluteExpiration = token.ExpireAt;
            await _cache.SetAsync($":TokenInfos:{tokenCode}", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(token)), cacheOption);
            return _currentToken;
        }

        public Guid? GetTokenCode()
        {
            var tokenCode = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (tokenCode == null)
                return null;
            return Guid.Parse(tokenCode);
        }

        public Guid GetUserCode()
        {
            var userCode = _httpContext.User.FindFirstValue(ClaimTypes.Sid);
            if (userCode == null)
                return Guid.Empty;
            return Guid.Parse(userCode);
        }

        public async Task<Tenant> GetCurrentTenantAsync()
        {
            ITenantIdentificationService identiticationService;

            //Kiểm tra URL, nếu có tham số ?tenantCode={id} thì lấy data theo Tenant này, còn ko lấy tenant theo domain
            if (!string.IsNullOrWhiteSpace(_httpContext.Request.Query["tenantCode"].ToString()))
                identiticationService = _identificationServices.First(x => x.GetType().Name == typeof(QueryTenantService).Name);
            else
                identiticationService = _identificationServices.First(x => x.GetType().Name == typeof(HostTenantService).Name);

            return await identiticationService.GetCurrentTenantAsync(_httpContext);
        }

        public async Task<Guid> GetTenantCodeAsync()
        {
            var tenant = await GetCurrentTenantAsync();
            if (tenant != null)
                return tenant.Code;

            return Guid.Empty;
        }

        public Guid GetTenantCode()
        {
            var tenantCode = _httpContext.User.FindFirstValue(ClaimTypes.GroupSid);
            if (tenantCode != null)
                return Guid.Parse(tenantCode);
            return Guid.Empty;
        }

        public async Task<bool> HasClaimsAsync(List<string> claims)
        {
            var session = await GetSessionAsync();
            if (session == null)
                return false;

            if (session.Claims.ContainsKey(nameof(SpecialPolicy.Special_DoEnything)) || session.Claims.Any(x => claims.Contains(x.Key)))
                return true;
            return false;
        }

        public async Task<List<string>> GetClaimsAsync(string prefix)
        {
            var claims = new List<string>();

            var specials = typeof(SpecialPolicy).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
            claims.AddRange(specials.Select(x => x.Name).ToList());

            var session = await GetSessionAsync();
            if (session == null)
                return claims;

            claims.AddRange(session.Claims.Keys.Where(x => x.StartsWith(prefix)).ToList());
            return claims;
        }

        public async Task<KeyValuePair<ClaimOfResource, ClaimOfChildResource>> GetClaimOfResourceAsync(string claim)
        {
            var session = await GetSessionAsync();
            if (session == null)
                return new KeyValuePair<ClaimOfResource, ClaimOfChildResource>(default, default);

            if (!session.Claims.ContainsKey(claim))
                return new KeyValuePair<ClaimOfResource, ClaimOfChildResource>(default, default);

            return session.Claims[claim];
        }

        public async Task<ContextModel> GetContextAsync(string policy)
        {
            var session = await GetSessionAsync();
            if (session == null)
                return null;

            KeyValuePair<ClaimOfResource, ClaimOfChildResource> claim = new KeyValuePair<ClaimOfResource, ClaimOfChildResource>(default(ClaimOfResource), default(ClaimOfChildResource));
            if (session.Claims.ContainsKey(policy))
                claim = session.Claims[policy];

            var context = new ContextModel
            {
                Session = session,
                TenantCode = await GetTenantCodeAsync(),
                IdUser = GetUserCode(),
                ClaimOfOperation = policy,
                ClaimOfResource = claim.Key,
                ClaimOfChildResource = claim.Value
            };
            return context;
        }

        public void SetCachePerRequest<T>(string key, T data)
        {
            _httpContext.Items.TryAdd(key, data);
        }

        public async Task<T> GetOrAddCachePerRequestAsync<T>(string key, Func<Task<T>> builder)
        {
            if (_httpContext.Items.TryGetValue(key, out object data))
                return (T)data;

            data = await builder();
            SetCachePerRequest(key, data);
            return (T)data;
        }

        public string GetConnectionId()
        {
            var connectionId = _httpContext.Request.Headers["Connection-Id"];
            if (!string.IsNullOrWhiteSpace(connectionId))
                return connectionId.ToString();
            return null;
        }
    }
}
