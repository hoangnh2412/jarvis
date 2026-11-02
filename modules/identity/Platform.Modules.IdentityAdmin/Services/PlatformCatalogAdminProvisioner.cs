using Platform.Authorization.EntityFramework.Persistence;
using Platform.Modules.IdentityAdmin.Authorization;
using Platform.Tenants.Services;
using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Tenants.Models;
using Platform.Tenants.Services;

namespace Platform.Modules.IdentityAdmin.Services;

public sealed class PlatformCatalogAdminProvisioner(
    ITenantManager tenants,
    IAppIdentityStore tenantIdentity,
    IPasswordHasher<User> passwordHasher,
    TenantBusinessRoleCopy roles) : IPlatformCatalogAdminProvisioner
{
    public Task<CatalogAdminProvisionResult> ProvisionTenantAdminAsync(
        Guid tenantId,
        CatalogAdminAccountInput admin,
        CancellationToken cancellationToken = default) =>
        ProvisionAsync(
            admin,
            tenantId,
            orgId: null,
            SystemAuthorizationAligner.TenantAdminRole,
            cancellationToken);

    public Task<CatalogAdminProvisionResult> ProvisionOrgAdminAsync(
        Guid tenantId,
        Guid orgId,
        CatalogAdminAccountInput admin,
        CancellationToken cancellationToken = default) =>
        ProvisionAsync(
            admin,
            tenantId,
            orgId,
            SystemAuthorizationAligner.OrgAdminRole,
            cancellationToken);

    private async Task<CatalogAdminProvisionResult> ProvisionAsync(
        CatalogAdminAccountInput admin,
        Guid tenantId,
        Guid? orgId,
        string roleName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var username = NormalizeRequired(admin.Username, 256);
        var email = NormalizeEmail(admin.Email, username);
        var password = NormalizeRequired(admin.Password, 128);
        var displayName = string.IsNullOrWhiteSpace(admin.DisplayName) ? null : admin.DisplayName.Trim();

        var tenantDb = tenantIdentity.DbContext;

        _ = await tenants.GetByIdAsync(tenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Không thấy tenant vừa tạo trên Master.");

        var role = await roles.EnsureAsync(roleName, cancellationToken).ConfigureAwait(false);
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            UserName = username,
            Email = email,
            NormalizedUserName = username.ToUpperInvariant(),
            NormalizedEmail = email.ToUpperInvariant(),
            TenantId = tenantId,
            OrgId = orgId,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
        };
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        tenantDb.Users.Add(user);
        tenantDb.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = role.Id });
        tenantDb.UserClaims.Add(new IdentityUserClaim<Guid>
        {
            UserId = user.Id,
            ClaimType = PermissionCatalog.ClaimType,
            ClaimValue = PlatformPermissions.ShellAccess,
        });
        if (displayName is not null)
        {
            tenantDb.UserClaims.Add(new IdentityUserClaim<Guid>
            {
                UserId = user.Id,
                ClaimType = IdentityClaimTypes.DisplayName,
                ClaimValue = displayName,
            });
        }

        await tenantDb.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new CatalogAdminProvisionResult
        {
            UserId = user.Id,
            Username = user.UserName!,
        };
    }

    private static string NormalizeRequired(string? value, int maxLength)
    {
        var text = value?.Trim() ?? string.Empty;
        if (text.Length == 0 || text.Length > maxLength)
            throw new ArgumentException("Thông tin tài khoản admin không hợp lệ.");
        return text;
    }

    private static string NormalizeEmail(string? email, string username)
    {
        var text = email?.Trim() ?? string.Empty;
        if (text.Length == 0)
            return $"{username}@catalog.local";
        return text;
    }
}
