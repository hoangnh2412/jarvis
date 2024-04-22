using System.Security.Claims;
using Jarvis.Shared.Extensions;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Application.MultiTenancy;

public class UserTenantIdResolver : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserTenantIdResolver(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, ClaimTypes.GroupSid);
        if (claim == null || claim.Value == null)
            return Guid.Empty;

        return Guid.Parse(claim.Value);
    }
}