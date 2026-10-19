using AspNetCore.Authentication.ApiKey;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.Authentication.ApiKey;
using Platform.DDD.Domain.Services;
using Platform.Multitenancy;
using Sample.Persistence;

namespace Sample.Authentication;

/// <summary>API Key — hash trong <see cref="Entities.ApiKeyCredential"/>, lọc theo tenant + profile.</summary>
public sealed class ApiKeyDbProvider(
    IServiceScopeFactory scopeFactory,
    IOptions<ApiKeyProviderOptions> providerOptions) : IApiKeyProvider
{
    public async Task<IApiKey?> ProvideAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        if (TryParseRealmKey(key, out var profile, out var secret))
            return await ValidateAsync(profile, secret, key);

        var defaultProfile = providerOptions.Value.DefaultRealm;
        return await ValidateAsync(defaultProfile, key, key);
    }

    private async Task<IApiKey?> ValidateAsync(string profile, string secret, string rawKey)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant<CurrentTenantInfo>>();
        var tenantId = await currentTenant.GetIdAsync()
            ?? throw new InvalidOperationException(
                "Tenant is required for API key validation. Send a valid tenant (e.g. X-Tenant-Id header).");

        var db = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var candidates = await db.ApiKeyCredentials
            .AsNoTracking()
            .Where(x => x.IsActive && x.TenantId == tenantId && x.Profile == profile)
            .ToListAsync();

        var row = candidates.FirstOrDefault(x => AuthenticationSecretHashing.VerifyApiKey(secret, x.KeyHash));
        if (row is null)
            return null;

        return new ApiKeyModel(rawKey, row.OwnerName ?? profile);
    }

    private static bool TryParseRealmKey(string key, out string profile, out string secret)
    {
        var index = key.IndexOf(':');
        if (index <= 0 || index >= key.Length - 1)
        {
            profile = string.Empty;
            secret = string.Empty;
            return false;
        }

        profile = key[..index];
        secret = key[(index + 1)..];
        return true;
    }
}
