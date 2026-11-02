using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;

namespace Platform.Modules.IdentityAdmin.Authorization;

/// <summary>
/// API vận hành: user global (TenantId/OrgId null) hoặc user có claim trực tiếp
/// <see cref="PlatformPermissions.TenantView"/> / <see cref="PlatformPermissions.TenantManage"/> trên bản ghi user.
/// Wildcard role không mở catalog tenant khi user còn gắn tenant/org — tránh TENANT_ADMIN lẫn quyền nền tảng.
/// </summary>
public sealed class PlatformRootRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public sealed class PlatformRootAuthorizationHandler(
    UserManager<User> users,
    IPermissionProvider permissions) : AuthorizationHandler<PlatformRootRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PlatformRootRequirement requirement)
    {
        var userId = ResolveUserId(context.User);
        if (userId is null)
            return;

        var user = await users.FindByIdAsync(userId.Value.ToString()).ConfigureAwait(false);
        if (user is null)
            return;

        var hasDirectPlatformClaim = await HasDirectPlatformTenantClaimAsync(user).ConfigureAwait(false);
        var isGlobalPrincipal = user.TenantId is null && user.OrgId is null;
        if (!isGlobalPrincipal && !hasDirectPlatformClaim)
            return;

        var allowed = await permissions.HasPermissionAsync(userId.Value, requirement.Permission).ConfigureAwait(false)
            || await permissions.HasPermissionAsync(userId.Value, PlatformPermissions.TenantManage).ConfigureAwait(false)
            || await permissions.HasPermissionAsync(userId.Value, PermissionCatalog.Wildcard).ConfigureAwait(false)
            || await permissions.HasPermissionAsync(userId.Value, PermissionCatalog.SystemAll).ConfigureAwait(false);

        if (allowed)
            context.Succeed(requirement);
    }

    private async Task<bool> HasDirectPlatformTenantClaimAsync(User user)
    {
        var claims = await users.GetClaimsAsync(user).ConfigureAwait(false);

        return claims.Any(claim =>
            string.Equals(claim.Type, PermissionCatalog.ClaimType, StringComparison.Ordinal)
            && (string.Equals(claim.Value, PlatformPermissions.TenantView, StringComparison.Ordinal)
                || string.Equals(claim.Value, PlatformPermissions.TenantManage, StringComparison.Ordinal)));
    }

    private static Guid? ResolveUserId(ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
        return Guid.TryParse(raw, out var userId) ? userId : null;
    }
}
