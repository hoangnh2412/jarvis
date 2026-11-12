using Platform.Authorization.EntityFramework.Persistence;
using Platform.Modules.IdentityAdmin.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Modules.IdentityAdmin.Authorization;

namespace Platform.Modules.IdentityAdmin.Authorization;

/// <summary>
/// Đưa claim của ba role hệ thống về use case Jarvis §5.8, và bảo đảm đã có một user ADMIN.
/// Wildcard chỉ ở ADMIN: engine bỏ qua ABAC khi role cấp quyền mang <c>*</c>.
/// </summary>
public sealed class SystemAuthorizationAligner(
    IPlatformIdentityStore store,
    UserManager<User> userManager,
    AuthorizationDataSeeder seeder,
    IPermissionCatalogProvider permissionCatalog)
{
    public const string AdminRole = "ADMIN";

    public const string TenantAdminRole = "TENANT_ADMIN";

    public const string OrgAdminRole = "ORG_ADMIN";

    public const string SameTenantPolicy = "SAME_TENANT";

    public const string SameOrgPolicy = "SAME_ORG";

    public async Task AlignAsync(CancellationToken cancellationToken = default)
    {
        var context = GetDb();
        await seeder.SeedAsync(cancellationToken).ConfigureAwait(false);

        var tenantAdminId = await FindRoleIdAsync(context, TenantAdminRole, cancellationToken).ConfigureAwait(false);
        var orgAdminId = await FindRoleIdAsync(context, OrgAdminRole, cancellationToken).ConfigureAwait(false);
        var codes = permissionCatalog.GetAllPermissions()
            .Select(permission => permission.Code)
            .Where(code =>
                !string.Equals(code, PlatformPermissions.TenantView, StringComparison.Ordinal)
                && !string.Equals(code, PlatformPermissions.TenantManage, StringComparison.Ordinal))
            .ToArray();

        if (tenantAdminId is Guid tenantRoleId)
        {
            await EnsurePermissionsAsync(context, tenantRoleId, codes, cancellationToken).ConfigureAwait(false);
            await EnsurePolicyAsync(context, tenantRoleId, SameTenantPolicy, cancellationToken).ConfigureAwait(false);
            await EnsurePermissionsAsync(
                context,
                tenantRoleId,
                [SystemPermissions.OrgView, SystemPermissions.OrgManage],
                cancellationToken).ConfigureAwait(false);
        }

        if (orgAdminId is Guid orgRoleId)
        {
            await EnsurePermissionsAsync(context, orgRoleId, codes, cancellationToken).ConfigureAwait(false);
            await EnsurePolicyAsync(context, orgRoleId, SameOrgPolicy, cancellationToken).ConfigureAwait(false);
            await EnsurePermissionsAsync(
                context,
                orgRoleId,
                [SystemPermissions.OrgView, SystemPermissions.OrgManage],
                cancellationToken).ConfigureAwait(false);
        }

        await EnsureBootstrapAdminAsync(cancellationToken).ConfigureAwait(false);
        await EnsurePlatformTenantClaimsForAdminUsersAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsurePlatformTenantClaimsForAdminUsersAsync(CancellationToken cancellationToken)
    {
        var targets = await userManager.GetUsersInRoleAsync(AdminRole).ConfigureAwait(false);
        if (targets.Count == 0)
            targets = await userManager.GetUsersInRoleAsync(TenantAdminRole).ConfigureAwait(false);

        foreach (var user in targets)
        {
            await EnsureUserPermissionClaimAsync(user, PlatformPermissions.TenantView, cancellationToken)
                .ConfigureAwait(false);
            await EnsureUserPermissionClaimAsync(user, PlatformPermissions.TenantManage, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task EnsureUserPermissionClaimAsync(
        User user,
        string permissionCode,
        CancellationToken cancellationToken)
    {
        var claims = await userManager.GetClaimsAsync(user).ConfigureAwait(false);
        if (claims.Any(claim =>
                string.Equals(claim.Type, PermissionCatalog.ClaimType, StringComparison.Ordinal)
                && string.Equals(claim.Value, permissionCode, StringComparison.Ordinal)))
        {
            return;
        }

        var added = await userManager.AddClaimAsync(
            user,
            new System.Security.Claims.Claim(PermissionCatalog.ClaimType, permissionCode))
            .ConfigureAwait(false);
        if (!added.Succeeded)
        {
            throw new InvalidOperationException(
                $"Không gán được claim {permissionCode} cho user {user.UserName}: "
                + string.Join("; ", added.Errors.Select(error => error.Description)));
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private async Task EnsureBootstrapAdminAsync(CancellationToken cancellationToken)
    {
        if ((await userManager.GetUsersInRoleAsync(AdminRole).ConfigureAwait(false)).Count > 0)
            return;

        var tenantAdmins = await userManager.GetUsersInRoleAsync(TenantAdminRole).ConfigureAwait(false);
        var bootstrap = tenantAdmins.OrderBy(user => user.Id).FirstOrDefault();
        if (bootstrap is null)
            return;

        var assigned = await userManager.AddToRoleAsync(bootstrap, AdminRole).ConfigureAwait(false);
        if (!assigned.Succeeded)
        {
            throw new InvalidOperationException(
                "Không gán được role ADMIN cho tài khoản cài đặt: "
                + string.Join("; ", assigned.Errors.Select(error => error.Description)));
        }
    }

    private static async Task<Guid?> FindRoleIdAsync(
        IdentityDbContextBase context,
        string normalizedName,
        CancellationToken cancellationToken)
    {
        return await context.Roles
            .Where(role => role.NormalizedName == normalizedName)
            .Select(role => (Guid?)role.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task EnsurePermissionsAsync(
        IdentityDbContextBase context,
        Guid roleId,
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken)
    {
        var existing = await context.RoleClaims
            .Where(claim => claim.RoleId == roleId && claim.ClaimType == PermissionCatalog.ClaimType)
            .Select(claim => claim.ClaimValue)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var held = new HashSet<string>(existing.Where(value => value != null)!, StringComparer.Ordinal);

        var added = false;
        foreach (var code in codes)
        {
            if (held.Contains(code))
                continue;

            context.RoleClaims.Add(new IdentityRoleClaim<Guid>
            {
                RoleId = roleId,
                ClaimType = PermissionCatalog.ClaimType,
                ClaimValue = code,
            });
            added = true;
        }

        if (added)
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsurePolicyAsync(
        IdentityDbContextBase context,
        Guid roleId,
        string policyCode,
        CancellationToken cancellationToken)
    {
        var exists = await context.RoleClaims.AnyAsync(
            claim => claim.RoleId == roleId && claim.ClaimType == "Policy" && claim.ClaimValue == policyCode,
            cancellationToken).ConfigureAwait(false);
        if (exists)
            return;

        context.RoleClaims.Add(new IdentityRoleClaim<Guid>
        {
            RoleId = roleId,
            ClaimType = "Policy",
            ClaimValue = policyCode,
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private IdentityDbContextBase GetDb() => store.DbContext;
}
