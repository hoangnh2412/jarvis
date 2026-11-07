using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Platform.Authentication;
using Platform.DDD.Domain.Shared.Extensions;
using Platform.Authentication.Identity.Domain;
using Platform.Authentication.Jwt;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Modules.Account.Abstractions;
using Platform.Orgs.Abstractions;
using Platform.Tenants.Services;

namespace Platform.Modules.Account.Services;

/// <summary>
/// Đăng nhập tầng App: tra tenant/org trên catalog platform, xác thực user trên identity store của tenant,
/// phát JWT Bearer. Store cụ thể do host cắm qua <see cref="IAppIdentityStore"/>.
/// </summary>
public sealed class AppLoginService(
    ITenantManager tenants,
    IOrgCatalog orgs,
    IAppIdentityStore tenantIdentity,
    IConfiguration configuration,
    IPasswordHasher<User> passwordHasher,
    IOptionsMonitor<AuthenticationJwtOption> jwtOptions) : IAppLoginService
{
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(14);

    public async Task<AppLoginResult> LoginAsync(
        string tenantCode,
        string orgCode,
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var tenant = await tenants.GetByCodeAsync(tenantCode.Trim(), cancellationToken).ConfigureAwait(false);
        if (tenant is null || !tenant.IsActive)
            throw new UnauthorizedAccessException();

        var org = await orgs.GetByCodeAsync(orgCode.Trim(), tenant.Id, cancellationToken).ConfigureAwait(false);
        if (org is null || !org.IsActive)
            throw new UnauthorizedAccessException();

        var tenantDb = tenantIdentity.DbContext;
        var normalized = userName.Trim().ToUpperInvariant();
        var user = await tenantDb.Users
            .FirstOrDefaultAsync(
                row => row.NormalizedUserName == normalized && row.TenantId == tenant.Id,
                cancellationToken)
            .ConfigureAwait(false);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            throw new UnauthorizedAccessException();

        if (user.LockoutEnd is DateTimeOffset lockedUntil && lockedUntil > DateTimeOffset.UtcNow)
            throw new UnauthorizedAccessException();

        var verified = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verified == PasswordVerificationResult.Failed)
        {
            user.AccessFailedCount += 1;
            await tenantDb.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            throw new UnauthorizedAccessException();
        }

        if (user.OrgId is not Guid assignedOrg || assignedOrg != org.Id)
            throw new UnauthorizedAccessException();

        if (user.AccessFailedCount != 0)
        {
            user.AccessFailedCount = 0;
            await tenantDb.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var db = tenantIdentity.DbContext;
        var refreshToken = await IssueRefreshTokenAsync(db, user.Id, cancellationToken).ConfigureAwait(false);
        return new AppLoginResult(IssueToken(user), refreshToken, user.Id, tenant.Id, org.Id);
    }

    public async Task<AppLoginResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException();

        var db = tenantIdentity.DbContext;
        var hash = HashRefreshToken(refreshToken.Trim());
        var row = await db.Set<UserRefreshToken>()
            .FirstOrDefaultAsync(item => item.TokenHash == hash, cancellationToken)
            .ConfigureAwait(false);
        if (row is null || row.RevokedAtUtc is not null || row.ExpiresAtUtc <= DateTime.UtcNow)
            throw new UnauthorizedAccessException();

        var user = await db.Users
            .FirstOrDefaultAsync(item => item.Id == row.UserId, cancellationToken)
            .ConfigureAwait(false);
        if (user is null || user.TenantId is not Guid tenantId || user.OrgId is not Guid orgId)
            throw new UnauthorizedAccessException();

        row.RevokedAtUtc = DateTimeExtension.UtcTimestamp();
        var nextRefreshToken = await IssueRefreshTokenAsync(db, user.Id, cancellationToken).ConfigureAwait(false);
        return new AppLoginResult(IssueToken(user), nextRefreshToken, user.Id, tenantId, orgId);
    }

    private static async Task<string> IssueRefreshTokenAsync(
        DbContext db,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var refreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        db.Set<UserRefreshToken>().Add(new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = HashRefreshToken(refreshToken),
            CreatedAtUtc = DateTimeExtension.UtcTimestamp(),
            ExpiresAtUtc = DateTimeExtension.UtcTimestamp().Add(RefreshLifetime),
        });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return refreshToken;
    }

    private static string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    private string IssueToken(User user)
    {
        var jwtOption = jwtOptions.Get(PlatformAuthenticationSchemes.Bearer);
        if (jwtOption is null || jwtOption.IssuerSigningKeys.Length == 0)
            throw new InvalidOperationException("JWT signing key not configured");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.IssuerSigningKeys[0]));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var tenantClaimName = configuration.GetValue<string>("TenantClaimName") ?? ClaimTypes.GroupSid;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("sub", user.Id.ToString()),
            new("jti", Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new("realm", AccountRealms.App),
        };
        if (user.TenantId is Guid tenantId)
            claims.Add(new Claim(tenantClaimName, tenantId.ToString()));

        var lifetime = jwtOption.MaxExpireMinutes > 0
            ? TimeSpan.FromMinutes(jwtOption.MaxExpireMinutes)
            : TimeSpan.FromHours(1);
        var issuer = jwtOption.ValidIssuers.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        var audience = !string.IsNullOrWhiteSpace(jwtOption.Audience)
            ? jwtOption.Audience
            : jwtOption.ValidAudiences.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
