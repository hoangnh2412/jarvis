using System.Security.Claims;
using Jarvis.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.DataStorages;

public class UserTenantIdResolver(
    IHttpContextAccessor httpContextAccessor,
    string claimName = ClaimTypes.GroupSid)
    : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? GetTenantId()
    {
        if (_httpContextAccessor.HttpContext == null)
            return null;

        var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, claimName);
        if (claim == null || claim.Value == null)
            return null;

        return claim.Value;
    }

    public Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetTenantId());
    }
}