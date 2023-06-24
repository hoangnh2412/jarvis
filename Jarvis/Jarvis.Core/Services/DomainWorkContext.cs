using Microsoft.AspNetCore.Http;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Jarvis.Core.Services
{
    public interface IDomainWorkContext
    {
        HttpRequest GetHttpRequest();

        Guid GetTokenKey();

        Guid GetTenantKey();

        Guid GetUserKey();

        string GetUserName();

        string GetUserFullName();
    }

    public class DomainWorkContext : IDomainWorkContext
    {
        private readonly HttpContext _httpContext;

        public DomainWorkContext(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        public HttpRequest GetHttpRequest()
        {
            return _httpContext.Request;
        }

        public Guid GetTokenKey()
        {
            var tokenId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (tokenId == null)
                return Guid.Empty;

            return Guid.Parse(tokenId);
        }

        public Guid GetTenantKey()
        {
            var tenantId = _httpContext.User.FindFirstValue(ClaimTypes.GroupSid);
            if (tenantId == null)
                return Guid.Empty;

            return Guid.Parse(tenantId);
        }

        public Guid GetUserKey()
        {
            var userId = _httpContext.User.FindFirstValue(ClaimTypes.Sid);
            if (userId == null)
                return Guid.Empty;

            return Guid.Parse(userId);
        }

        public string GetUserName()
        {
            return _httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public string GetUserFullName()
        {
            return _httpContext.User.FindFirstValue(ClaimTypes.Name);
        }
    }
}
