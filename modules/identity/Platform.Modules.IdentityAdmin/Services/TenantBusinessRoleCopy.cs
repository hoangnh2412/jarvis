using Platform.Authorization.EntityFramework.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Domain;

namespace Platform.Modules.IdentityAdmin.Services;

/// <summary>Nhân bản role và policy từ Master sang database nghiệp vụ của tenant.</summary>
public sealed class TenantBusinessRoleCopy(
    IPlatformIdentityStore masterStore,
    IAppIdentityStore tenantStore)
{
    public async Task<Role> EnsureAsync(
        string roleName,
        CancellationToken cancellationToken)
    {
        var master = masterStore.DbContext;
        var tenantDb = tenantStore.DbContext;

        var normalized = roleName.ToUpperInvariant();
        var existing = await tenantDb.Roles
            .FirstOrDefaultAsync(role => role.NormalizedName == normalized, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
            return existing;

        var source = await master.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(role => role.NormalizedName == normalized, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Thiếu role hệ thống {roleName} trên Master.");

        var copy = new Role
        {
            Id = source.Id,
            Name = source.Name,
            NormalizedName = source.NormalizedName,
            RoleLevel = source.RoleLevel,
            IsSystemRole = source.IsSystemRole,
            DisplayName = source.DisplayName,
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = source.CreatedAt,
        };
        tenantDb.Roles.Add(copy);

        var claims = await master.RoleClaims
            .AsNoTracking()
            .Where(claim => claim.RoleId == source.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (var claim in claims)
        {
            tenantDb.RoleClaims.Add(new IdentityRoleClaim<Guid>
            {
                RoleId = copy.Id,
                ClaimType = claim.ClaimType,
                ClaimValue = claim.ClaimValue,
            });
        }

        var policyCodes = claims
            .Where(claim => string.Equals(claim.ClaimType, "Policy", StringComparison.Ordinal) && claim.ClaimValue is not null)
            .Select(claim => claim.ClaimValue!)
            .ToArray();
        await CopyPoliciesAsync(master, tenantDb, policyCodes, cancellationToken).ConfigureAwait(false);
        await tenantDb.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return copy;
    }

    private static async Task CopyPoliciesAsync(
        IdentityDbContextBase master,
        IdentityDbContextBase tenantDb,
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken)
    {
        if (codes.Count == 0)
            return;

        var present = await tenantDb.Policies
            .Where(policy => codes.Contains(policy.Code))
            .Select(policy => policy.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var missing = codes.Where(code => !present.Contains(code)).ToArray();
        if (missing.Length == 0)
            return;

        var sources = await master.Policies
            .AsNoTracking()
            .Where(policy => missing.Contains(policy.Code))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var sourceIds = sources.Select(policy => policy.Id).ToArray();
        var rules = await master.PolicyRules
            .AsNoTracking()
            .Where(rule => sourceIds.Contains(rule.PolicyId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var source in sources)
        {
            tenantDb.Policies.Add(new Policy
            {
                Id = source.Id,
                Code = source.Code,
                Name = source.Name,
                ResourceType = source.ResourceType,
                Action = source.Action,
                IsSystemPolicy = source.IsSystemPolicy,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
            });
        }

        foreach (var rule in rules)
        {
            tenantDb.PolicyRules.Add(new PolicyRule
            {
                Id = rule.Id,
                PolicyId = rule.PolicyId,
                LeftOperand = rule.LeftOperand,
                Operator = rule.Operator,
                RightOperand = rule.RightOperand,
                Order = rule.Order,
                CreatedAt = rule.CreatedAt,
            });
        }
    }

}
