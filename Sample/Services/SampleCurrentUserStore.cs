using System.Security.Claims;
using Jarvis.Authentication;
using Jarvis.DDD.Domain.Services;
using Jarvis.DDD.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Sample.Services;

/// <summary>
/// Store demo cho Sample: dựng <see cref="CurrentUserInfo"/> từ claims HTTP.
/// Host thật thay bằng DB/cache (không dùng null-store / factory fallback).
/// </summary>
public sealed class SampleCurrentUserStore(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : ICurrentUserStore<CurrentUserInfo>
{
    private readonly string _tenantClaimName =
        configuration.GetValue<string>("TenantClaimName") ?? ClaimTypes.GroupSid;

    public Task<CurrentUserInfo?> FindAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var principal = httpContextAccessor.HttpContext?.User;
        if (principal == null)
            return Task.FromResult<CurrentUserInfo?>(null);

        return Task.FromResult<CurrentUserInfo?>(new CurrentUserInfo
        {
            UserId = userId,
            TokenId = ParseGuidClaim(principal, "jti"),
            TenantId = ParseGuidClaim(principal, _tenantClaimName),
            UserName = principal.Identity?.Name,
        });
    }

    private static Guid? ParseGuidClaim(ClaimsPrincipal principal, string claimName)
    {
        var value = ClaimsPrincipalExtension.GetClaim(principal.Claims, claimName)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
