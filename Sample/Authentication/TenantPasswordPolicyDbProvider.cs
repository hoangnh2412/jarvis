using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Platform.Authentication;
using Sample.Persistence;

namespace Sample.Authentication;

/// <summary>Password policy theo tenant — bảng <see cref="Entities.TenantPasswordPolicy"/>.</summary>
public sealed class TenantPasswordPolicyDbProvider(IServiceScopeFactory scopeFactory) : IPasswordPolicyProvider
{
    public async Task<PasswordPolicyOptions?> GetPolicyAsync(PasswordPolicyScope scope, CancellationToken cancellationToken = default)
    {
        if (scope.TenantId is null)
            return null;

        await using var diScope = scopeFactory.CreateAsyncScope();
        var db = diScope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var row = await db.TenantPasswordPolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == scope.TenantId.Value, cancellationToken);

        return row?.ToOptions();
    }
}
