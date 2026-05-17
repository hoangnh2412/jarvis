using System.Security.Claims;
using Jarvis.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Jarvis.Domain.DataStorages;

public class UserTenantIdResolver(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration)
    : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly string _tenantClaimName = configuration.GetValue<string>("TenantClaimName") ?? ClaimTypes.GroupSid;

    public Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_httpContextAccessor.HttpContext == null)
            return Task.FromResult<Guid?>(null);

        var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, _tenantClaimName);
        return Task.FromResult(TenantIdGuidParser.Parse(claim?.Value));
    }
}
