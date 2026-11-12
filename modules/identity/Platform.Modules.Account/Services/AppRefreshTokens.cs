using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.DDD.Domain.Shared.Extensions;

namespace Platform.Modules.Account.Services;

internal static class AppRefreshTokens
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(14);

    public static async Task<string> IssueAsync(DbContext db, Guid userId, CancellationToken cancellationToken)
    {
        var refreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        db.Set<UserRefreshToken>().Add(new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Hash(refreshToken),
            CreatedAtUtc = DateTimeExtension.UtcTimestamp(),
            ExpiresAtUtc = DateTimeExtension.UtcTimestamp().Add(Lifetime),
        });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return refreshToken;
    }

    public static string Hash(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
