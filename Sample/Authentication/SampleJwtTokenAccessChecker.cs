using System.Security.Claims;
using Jarvis.Authentication.Jwt;

namespace Sample.Authentication;

/// <summary>
/// Mẫu <see cref="IJwtTokenAccessChecker"/> — luôn allow.
/// Production: tra Redis/DB blacklist theo claim <c>jti</c> (hoặc whitelist) trong <see cref="IsAllowedAsync"/>.
/// </summary>
/// <example>
/// <code>
/// auth.AddCoreJwtBearer&lt;SampleJwtTokenAccessChecker&gt;(configuration);
/// // Blacklist: var jti = principal.FindFirst("jti")?.Value;
/// // return jti is not null &amp;&amp; !await revokedStore.ContainsAsync(jti, ct);
/// </code>
/// </example>
public sealed class SampleJwtTokenAccessChecker : IJwtTokenAccessChecker
{
    public Task<bool> IsAllowedAsync(
        ClaimsPrincipal principal,
        string? rawToken,
        CancellationToken cancellationToken = default)
    {
        _ = principal;
        _ = rawToken;
        _ = cancellationToken;
        return Task.FromResult(true);
    }
}
