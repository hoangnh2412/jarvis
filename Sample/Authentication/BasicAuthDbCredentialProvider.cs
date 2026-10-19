using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Basic;
using Platform.DDD.Domain.Services;
using Platform.Multitenancy;
using Sample.Persistence;

namespace Sample.Authentication;

/// <summary>HTTP Basic — <see cref="Entities.BasicAuthUser"/> (hash) theo tenant.</summary>
public sealed class BasicAuthDbCredentialProvider(IServiceScopeFactory scopeFactory) : IBasicCredentialProvider
{
    public async Task<BasicValidationResult?> AuthenticateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return null;

        await using var scope = scopeFactory.CreateAsyncScope();
        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant<CurrentTenantInfo>>();
        var tenantId = await currentTenant.GetIdAsync(cancellationToken)
            ?? throw new InvalidOperationException("Tenant is required for Basic authentication. Send a valid tenant (e.g. X-Tenant-Id header).");

        var db = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var user = await db.BasicAuthUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.TenantId == tenantId && u.Username == username,
                cancellationToken);

        if (user is null)
            return null;

        if (!AuthenticationSecretHashing.VerifyBasicPassword(password, user.PasswordHash))
            return null;

        return BasicValidationResult.FromVerifiedUser(username, user.Roles, user.Id.ToString());
    }
}
