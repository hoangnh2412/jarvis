using System.Security.Claims;
using Platform.Authorization.EntityFramework.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity.Domain;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.IdentityAdmin.Abstractions;
using Platform.Modules.IdentityAdmin.Authorization;

namespace Platform.Modules.IdentityAdmin.Services;

public sealed class PlatformUserAdminService(
    IPlatformIdentityStore store,
    UserManager<User> userManager,
    RoleManager<Role> roleManager) : IPlatformUserAdminService
{
    public async Task<IReadOnlyList<PlatformUserListItem>> ListAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var context = GetDb();
        var query = context.Users.AsNoTracking()
            .Where(user => user.TenantId == null && user.OrgId == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(user =>
                (user.UserName != null && user.UserName.Contains(term))
                || (user.Email != null && user.Email.Contains(term)));
        }

        var users = await query
            .OrderBy(user => user.UserName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (users.Count == 0)
            return [];

        var userIds = users.Select(user => user.Id).ToArray();
        var roleRows = await context.UserRoles.AsNoTracking()
            .Where(userRole => userIds.Contains(userRole.UserId))
            .Join(
                context.Roles.AsNoTracking(),
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => new { userRole.UserId, RoleName = role.Name ?? string.Empty })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var rolesByUser = roleRows
            .GroupBy(row => row.UserId)
            .ToDictionary(group => group.Key, group => group.Select(row => row.RoleName).Where(name => name.Length > 0).ToArray());

        var displayClaims = await context.UserClaims.AsNoTracking()
            .Where(claim =>
                userIds.Contains(claim.UserId)
                && claim.ClaimType == IdentityInstallService.DisplayNameClaimType)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var displayByUser = displayClaims
            .GroupBy(claim => claim.UserId)
            .ToDictionary(group => group.Key, group => group.First().ClaimValue);

        return users.Select(user =>
        {
            rolesByUser.TryGetValue(user.Id, out var roleNames);
            displayByUser.TryGetValue(user.Id, out var displayName);
            return new PlatformUserListItem(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email,
                displayName,
                user.PhoneNumber,
                user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow,
                roleNames ?? Array.Empty<string>());
        }).ToList();
    }

    public async Task<PlatformUserListItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var context = GetDb();
        var user = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(
                candidate => candidate.Id == id && candidate.TenantId == null && candidate.OrgId == null,
                cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
            return null;

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        var locked = await userManager.IsLockedOutAsync(user).ConfigureAwait(false);
        var displayClaims = await userManager.GetClaimsAsync(user).ConfigureAwait(false);
        var displayName = displayClaims.FirstOrDefault(claim =>
            string.Equals(claim.Type, IdentityInstallService.DisplayNameClaimType, StringComparison.Ordinal))?.Value;

        return new PlatformUserListItem(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email,
            displayName,
            user.PhoneNumber,
            locked,
            roles.ToArray());
    }

    public async Task<PlatformUserListItem> CreateAsync(
        CreatePlatformUserRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var username = request.Username.Trim();
        var email = string.IsNullOrWhiteSpace(request.Email) ? $"{username}@local" : request.Email.Trim();
        var password = request.Password;
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim();

        var user = new User
        {
            Id = Guid.CreateVersion7(),
            UserName = username,
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            NormalizedUserName = username.ToUpperInvariant(),
            NormalizedEmail = email.ToUpperInvariant(),
            TenantId = null,
            OrgId = null,
        };

        var created = await userManager.CreateAsync(user, password).ConfigureAwait(false);
        if (!created.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", created.Errors.Select(error => error.Description)));
        }

        var assignedRoleNames = new List<string>();
        if (request.RoleIds is { Count: > 0 })
        {
            foreach (var roleId in request.RoleIds.Where(id => id != Guid.Empty).Distinct())
            {
                var targetRole = await roleManager.FindByIdAsync(roleId.ToString()).ConfigureAwait(false)
                    ?? throw new InvalidOperationException($"Không tìm thấy role '{roleId}'.");
                var assigned = await userManager.AddToRoleAsync(user, targetRole.Name!).ConfigureAwait(false);
                if (!assigned.Succeeded)
                {
                    await userManager.DeleteAsync(user).ConfigureAwait(false);
                    throw new InvalidOperationException(
                        string.Join("; ", assigned.Errors.Select(error => error.Description)));
                }

                assignedRoleNames.Add(targetRole.Name!);
            }
        }
        else
        {
            var adminRole = await roleManager.FindByNameAsync(SystemAuthorizationAligner.AdminRole).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Thiếu role ADMIN.");

            var roleAssigned = await userManager.AddToRoleAsync(user, adminRole.Name!).ConfigureAwait(false);
            if (!roleAssigned.Succeeded)
            {
                await userManager.DeleteAsync(user).ConfigureAwait(false);
                throw new InvalidOperationException(
                    string.Join("; ", roleAssigned.Errors.Select(error => error.Description)));
            }

            assignedRoleNames.Add(adminRole.Name!);
        }

        await userManager.AddClaimAsync(user, new Claim(PermissionCatalog.ClaimType, PlatformPermissions.TenantView))
            .ConfigureAwait(false);
        await userManager.AddClaimAsync(user, new Claim(PermissionCatalog.ClaimType, PlatformPermissions.TenantManage))
            .ConfigureAwait(false);
        await userManager.AddClaimAsync(user, new Claim(PermissionCatalog.ClaimType, PlatformPermissions.ShellAccess))
            .ConfigureAwait(false);

        if (displayName is not null)
        {
            await userManager.AddClaimAsync(user, new Claim(IdentityInstallService.DisplayNameClaimType, displayName))
                .ConfigureAwait(false);
        }

        return new PlatformUserListItem(
            user.Id,
            user.UserName ?? username,
            user.Email,
            displayName,
            user.PhoneNumber,
            false,
            assignedRoleNames);
    }

    public async Task<PlatformUserListItem> UpdateAsync(
        Guid id,
        UpdatePlatformUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = GetDb();
        var user = await context.Users.FirstOrDefaultAsync(
                candidate => candidate.Id == id && candidate.TenantId == null && candidate.OrgId == null,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException(IdentityAdminErrorCode.PlatformUserNotFound);

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.Email = request.Email.Trim();
            user.NormalizedEmail = user.Email.ToUpperInvariant();
        }

        if (request.PhoneNumber is not null)
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
            var reset = await userManager.ResetPasswordAsync(user, token, request.Password).ConfigureAwait(false);
            if (!reset.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", reset.Errors.Select(error => error.Description)));
            }
        }

        if (request.IsLocked is not null)
        {
            if (request.IsLocked.Value)
                await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue).ConfigureAwait(false);
            else
                await userManager.SetLockoutEndDateAsync(user, null).ConfigureAwait(false);
        }

        await userManager.UpdateAsync(user).ConfigureAwait(false);

        if (request.RoleIds is not null)
        {
            var currentRoles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            if (currentRoles.Count > 0)
                await userManager.RemoveFromRolesAsync(user, currentRoles).ConfigureAwait(false);

            foreach (var roleId in request.RoleIds.Where(roleId => roleId != Guid.Empty).Distinct())
            {
                var targetRole = await roleManager.FindByIdAsync(roleId.ToString()).ConfigureAwait(false)
                    ?? throw new InvalidOperationException($"Không tìm thấy role '{roleId}'.");
                await userManager.AddToRoleAsync(user, targetRole.Name!).ConfigureAwait(false);
            }
        }

        if (request.DisplayName is not null)
        {
            var claims = await userManager.GetClaimsAsync(user).ConfigureAwait(false);
            var existing = claims.FirstOrDefault(claim =>
                string.Equals(claim.Type, IdentityInstallService.DisplayNameClaimType, StringComparison.Ordinal));
            if (existing is not null)
                await userManager.RemoveClaimAsync(user, existing).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(request.DisplayName))
            {
                await userManager.AddClaimAsync(
                        user,
                        new Claim(IdentityInstallService.DisplayNameClaimType, request.DisplayName.Trim()))
                    .ConfigureAwait(false);
            }
        }

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        var locked = await userManager.IsLockedOutAsync(user).ConfigureAwait(false);
        var displayClaims = await userManager.GetClaimsAsync(user).ConfigureAwait(false);
        var displayName = displayClaims.FirstOrDefault(claim =>
            string.Equals(claim.Type, IdentityInstallService.DisplayNameClaimType, StringComparison.Ordinal))?.Value;

        return new PlatformUserListItem(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email,
            displayName,
            user.PhoneNumber,
            locked,
            roles.ToArray());
    }

    public async Task<PlatformUserListItem> SetLockedAsync(
        Guid id,
        bool isLocked,
        CancellationToken cancellationToken = default)
    {
        var context = GetDb();
        var user = await context.Users.FirstOrDefaultAsync(
                candidate => candidate.Id == id && candidate.TenantId == null && candidate.OrgId == null,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException(IdentityAdminErrorCode.PlatformUserNotFound);

        if (isLocked)
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue).ConfigureAwait(false);
        else
            await userManager.SetLockoutEndDateAsync(user, null).ConfigureAwait(false);

        await userManager.UpdateAsync(user).ConfigureAwait(false);

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        var locked = await userManager.IsLockedOutAsync(user).ConfigureAwait(false);
        var displayClaims = await userManager.GetClaimsAsync(user).ConfigureAwait(false);
        var displayName = displayClaims.FirstOrDefault(claim =>
            string.Equals(claim.Type, IdentityInstallService.DisplayNameClaimType, StringComparison.Ordinal))?.Value;

        return new PlatformUserListItem(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email,
            displayName,
            user.PhoneNumber,
            locked,
            roles.ToArray());
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var context = GetDb();
        var user = await context.Users.FirstOrDefaultAsync(
                candidate => candidate.Id == id && candidate.TenantId == null && candidate.OrgId == null,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException(IdentityAdminErrorCode.PlatformUserNotFound);

        var deleted = await userManager.DeleteAsync(user).ConfigureAwait(false);
        if (!deleted.Succeeded)
        {
            throw new BusinessException(
                IdentityAdminErrorCode.PlatformUserDeleteFailed,
                string.Join("; ", deleted.Errors.Select(error => error.Description)));
        }
    }

    private IdentityDbContextBase GetDb() => store.DbContext;
}
