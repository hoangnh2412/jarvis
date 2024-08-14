using System.Security.Claims;
using Jarvis.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.DataStorages;

public class UserTenantIdResolver(
    IHttpContextAccessor httpContextAccessor)
    : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid Resolve()
    {
        if (_httpContextAccessor.HttpContext == null)
            return Guid.Empty;

        var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, ClaimTypes.GroupSid);
        if (claim == null || claim.Value == null)
            return Guid.Empty;

        return Guid.Parse(claim.Value);
    }

    public Task<Guid> ResolveAsync()
    {
        return Task.FromResult(Resolve());
    }
}