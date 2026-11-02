using System.Security.Claims;
using Platform.Authorization.EntityFramework.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity;
using Platform.Authentication.Identity.Domain;

namespace Platform.Modules.IdentityAdmin.Services;

/// <summary>Thao tác user/claim trên Master identity hoặc App identity (TD-08).</summary>
public sealed class RealmUserOperations(
    UserManager<User> platformUsers,
    IAppIdentityStore appIdentityStore)
{
    public async Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var platform = await platformUsers.FindByIdAsync(userId.ToString()).ConfigureAwait(false);
        if (platform is not null)
            return platform;

        var appIdentity = await GetAppIdentityDbAsync(cancellationToken).ConfigureAwait(false);
        return await appIdentity.Users
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<string?> GetDisplayNameAsync(User user, CancellationToken cancellationToken = default)
    {
        var appIdentity = await GetAppIdentityDbAsync(cancellationToken).ConfigureAwait(false);
        if (await IsAppUserAsync(appIdentity, user.Id, cancellationToken).ConfigureAwait(false))
        {
            var claim = await appIdentity.Set<IdentityUserClaim<Guid>>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    row => row.UserId == user.Id && row.ClaimType == IdentityClaimTypes.DisplayName,
                    cancellationToken)
                .ConfigureAwait(false);
            return claim?.ClaimValue;
        }

        var claims = await platformUsers.GetClaimsAsync(user).ConfigureAwait(false);
        return claims.FirstOrDefault(claim => claim.Type == IdentityClaimTypes.DisplayName)?.Value;
    }

    public async Task<string[]> GetRoleNamesAsync(User user, CancellationToken cancellationToken = default)
    {
        var appIdentity = await GetAppIdentityDbAsync(cancellationToken).ConfigureAwait(false);
        if (await IsAppUserAsync(appIdentity, user.Id, cancellationToken).ConfigureAwait(false))
        {
            var names = await appIdentity.UserRoles.AsNoTracking()
                .Where(userRole => userRole.UserId == user.Id)
                .Join(
                    appIdentity.Roles.AsNoTracking(),
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (_, role) => role.Name)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return OrderRoleNames(names);
        }

        var platformRoles = await platformUsers.GetRolesAsync(user).ConfigureAwait(false);
        return OrderRoleNames(platformRoles);
    }

    public async Task UpdateOrgScopeColumnsAsync(
        User user,
        Guid organizationalUnitId,
        string? unitType,
        CancellationToken cancellationToken = default)
    {
        user.OrganizationalUnitId = organizationalUnitId;
        user.UnitType = unitType;

        var appIdentity = await GetAppIdentityDbAsync(cancellationToken).ConfigureAwait(false);
        if (await IsAppUserAsync(appIdentity, user.Id, cancellationToken).ConfigureAwait(false))
        {
            appIdentity.Users.Update(user);
            await appIdentity.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await platformUsers.UpdateAsync(user).ConfigureAwait(false);
    }

    public async Task SyncOrgClaimsAsync(
        User user,
        Guid unitId,
        string unitType,
        string? legalEntityId,
        string path,
        Guid positionId,
        CancellationToken cancellationToken = default)
    {
        user.OrganizationalUnitId = unitId;
        user.UnitType = unitType;

        var appIdentity = await GetAppIdentityDbAsync(cancellationToken).ConfigureAwait(false);
        if (await IsAppUserAsync(appIdentity, user.Id, cancellationToken).ConfigureAwait(false))
        {
            appIdentity.Users.Update(user);
            await ReplaceClaimAsync(appIdentity, user.Id, "OrganizationalUnitId", unitId.ToString(), cancellationToken)
                .ConfigureAwait(false);
            await ReplaceClaimAsync(appIdentity, user.Id, "UnitType", unitType, cancellationToken).ConfigureAwait(false);
            await ReplaceClaimAsync(appIdentity, user.Id, "LegalEntityId", legalEntityId, cancellationToken)
                .ConfigureAwait(false);
            await ReplaceClaimAsync(appIdentity, user.Id, "OrganizationalUnitPath", path, cancellationToken)
                .ConfigureAwait(false);
            await ReplaceClaimAsync(
                    appIdentity,
                    user.Id,
                    "PositionId",
                    positionId == Guid.Empty ? null : positionId.ToString(),
                    cancellationToken)
                .ConfigureAwait(false);
            await appIdentity.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await platformUsers.UpdateAsync(user).ConfigureAwait(false);
        await ReplaceClaimAsync(platformUsers, user, "OrganizationalUnitId", unitId.ToString()).ConfigureAwait(false);
        await ReplaceClaimAsync(platformUsers, user, "UnitType", unitType).ConfigureAwait(false);
        await ReplaceClaimAsync(platformUsers, user, "LegalEntityId", legalEntityId).ConfigureAwait(false);
        await ReplaceClaimAsync(platformUsers, user, "OrganizationalUnitPath", path).ConfigureAwait(false);
        await ReplaceClaimAsync(
                platformUsers,
                user,
                "PositionId",
                positionId == Guid.Empty ? null : positionId.ToString())
            .ConfigureAwait(false);
    }

    /// <summary>Xóa claim OU và cột đơn vị khi user không còn membership active.</summary>
    public async Task ClearOrgScopeAsync(User user, CancellationToken cancellationToken = default)
    {
        user.OrganizationalUnitId = null;
        user.UnitType = null;

        var appIdentity = await GetAppIdentityDbAsync(cancellationToken).ConfigureAwait(false);
        if (await IsAppUserAsync(appIdentity, user.Id, cancellationToken).ConfigureAwait(false))
        {
            appIdentity.Users.Update(user);
            await ReplaceClaimAsync(appIdentity, user.Id, "OrganizationalUnitId", null, cancellationToken)
                .ConfigureAwait(false);
            await ReplaceClaimAsync(appIdentity, user.Id, "UnitType", null, cancellationToken).ConfigureAwait(false);
            await ReplaceClaimAsync(appIdentity, user.Id, "LegalEntityId", null, cancellationToken)
                .ConfigureAwait(false);
            await ReplaceClaimAsync(appIdentity, user.Id, "OrganizationalUnitPath", null, cancellationToken)
                .ConfigureAwait(false);
            await ReplaceClaimAsync(appIdentity, user.Id, "PositionId", null, cancellationToken).ConfigureAwait(false);
            await appIdentity.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await platformUsers.UpdateAsync(user).ConfigureAwait(false);
        await ReplaceClaimAsync(platformUsers, user, "OrganizationalUnitId", null).ConfigureAwait(false);
        await ReplaceClaimAsync(platformUsers, user, "UnitType", null).ConfigureAwait(false);
        await ReplaceClaimAsync(platformUsers, user, "LegalEntityId", null).ConfigureAwait(false);
        await ReplaceClaimAsync(platformUsers, user, "OrganizationalUnitPath", null).ConfigureAwait(false);
        await ReplaceClaimAsync(platformUsers, user, "PositionId", null).ConfigureAwait(false);
    }

    private Task<IdentityDbContextBase> GetAppIdentityDbAsync(CancellationToken cancellationToken) =>
        Task.FromResult(appIdentityStore.DbContext);

    private static async Task<bool> IsAppUserAsync(
        IdentityDbContextBase appIdentity,
        Guid userId,
        CancellationToken cancellationToken) =>
        await appIdentity.Users.AsNoTracking().AnyAsync(user => user.Id == userId, cancellationToken).ConfigureAwait(false);

    private static string[] OrderRoleNames(IEnumerable<string?> names) =>
        names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

    private static async Task ReplaceClaimAsync(
        UserManager<User> manager,
        User user,
        string type,
        string? value)
    {
        var existing = await manager.GetClaimsAsync(user).ConfigureAwait(false);
        foreach (var claim in existing.Where(claim => claim.Type == type))
            await manager.RemoveClaimAsync(user, claim).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(value))
            await manager.AddClaimAsync(user, new Claim(type, value)).ConfigureAwait(false);
    }

    private static async Task ReplaceClaimAsync(
        IdentityDbContextBase context,
        Guid userId,
        string type,
        string? value,
        CancellationToken cancellationToken)
    {
        var existing = await context.Set<IdentityUserClaim<Guid>>()
            .Where(claim => claim.UserId == userId && claim.ClaimType == type)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        context.RemoveRange(existing);
        if (!string.IsNullOrWhiteSpace(value))
        {
            context.Add(new IdentityUserClaim<Guid>
            {
                UserId = userId,
                ClaimType = type,
                ClaimValue = value,
            });
        }
    }
}
