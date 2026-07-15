using Sample.Persistence;
using Jarvis.Authentication.ApiKey;
using AspNetCore.Authentication.ApiKey;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Sample.Authentication;

/// <summary>
/// API key provider tra Master DB qua <see cref="IDbContextFactory{TContext}"/> (Singleton-safe).
/// </summary>
public sealed class SampleApiKeyProvider(IDbContextFactory<MasterDbContext> dbFactory) : IApiKeyProvider
{
    public async Task<IApiKey?> ProvideAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        await using var db = await dbFactory.CreateDbContextAsync();
        var row = await db.ApiKeyCredentials.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key);

        if (row is null)
            return null;

        var claims = new List<Claim>();
        foreach (var role in row.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return new ApiKeyModel(key, row.OwnerName, claims);
    }
}
