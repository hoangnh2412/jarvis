using System.Security.Claims;

namespace Jarvis.Authentication.Jwt;

/// <summary>
/// Kiểm tra token JWT đã validate (blacklist / whitelist / revoke) — nguồn do host cung cấp (DB, Redis, …).
/// </summary>
/// <remarks>
/// Gọi trong <c>JwtBearerEvents.OnTokenValidated</c> sau khi chữ ký và lifetime OK.
/// Đăng ký qua <c>AddCoreJwtBearer&lt;T&gt;</c>; mặc định <see cref="AllowAllJwtTokenAccessChecker"/>.
/// Lifetime DI: Singleton — dùng <c>IDbContextFactory</c> / <c>IServiceScopeFactory</c> nếu tra DB.
/// </remarks>
public interface IJwtTokenAccessChecker
{
    /// <summary>
    /// <c>true</c> nếu token được phép; <c>false</c> → authentication fail (revoked / not in whitelist).
    /// </summary>
    /// <param name="principal">Principal sau cryptographic validation.</param>
    /// <param name="rawToken">Bearer token thô (có thể null nếu không parse được header).</param>
    Task<bool> IsAllowedAsync(
        ClaimsPrincipal principal,
        string? rawToken,
        CancellationToken cancellationToken = default);
}
