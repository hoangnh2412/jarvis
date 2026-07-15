using System.Security.Claims;

namespace Jarvis.Authentication.Jwt;

/// <summary>Checker mặc định — luôn cho phép (không blacklist/whitelist).</summary>
public sealed class AllowAllJwtTokenAccessChecker : IJwtTokenAccessChecker
{
    public Task<bool> IsAllowedAsync(
        ClaimsPrincipal principal,
        string? rawToken,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}
