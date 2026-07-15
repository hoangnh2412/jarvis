using System.Security.Claims;
using Jarvis.Authentication.Basic;
using Microsoft.EntityFrameworkCore;
using Sample.Persistence;

namespace Sample.Authentication;

/// <summary>
/// Basic credential provider tra Master DB qua <see cref="IDbContextFactory{TContext}"/> (Singleton-safe).
/// Password seed là plaintext demo — production phải hash/verify trong provider.
/// </summary>
public sealed class SampleBasicAuthCredentialProvider(IDbContextFactory<MasterDbContext> dbFactory)
    : IBasicCredentialProvider
{
    public async Task<BasicValidationResult?> AuthenticateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return null;

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var user = await db.BasicAuthUsers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (user is null)
            return null;

        // Demo-only plaintext compare — production: bcrypt/Argon2 verify here.
        if (!string.Equals(user.Password, password, StringComparison.Ordinal))
            return null;

        var claims = new List<Claim> { new(ClaimTypes.Name, username) };
        foreach (var role in user.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return new BasicValidationResult
        {
            Username = username,
            Claims = claims
        };
    }
}
