using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Platform.Authorization.Abstractions;
using Platform.DDD.Domain.Services;
using Platform.Modules.Account.Abstractions;
using Platform.Modules.IdentityAdmin.Abstractions;
using Platform.Modules.IdentityAdmin.Authorization;
using Platform.Modules.IdentityAdmin.Services;
using Platform.OrgStructure.Models;
using Platform.OrgStructure.Services;
using Platform.Orgs.Services;
using Platform.Tenants.Services;

namespace Platform.Modules.Account.Services;

/// <summary>
/// Thông tin user sau đăng nhập (<c>GET /api/v1/me</c>) và gán phạm vi đơn vị cho user.
/// Đơn vị tổ chức đọc qua <see cref="IOrgStructureManager"/> — không phụ thuộc DbContext của host.
/// </summary>
public sealed class UserOrgScopeService(
    IHttpContextAccessor httpContextAccessor,
    IPermissionProvider permissions,
    IPermissionCatalogProvider permissionCatalog,
    RealmUserOperations realmUsers,
    IOrgStructureManager structure,
    ICurrentTenant<TenantCatalogProfile> currentTenant,
    ICurrentOrg<OrgCatalogOrgProfile> currentOrg) : IAccountMeService, IUserOrgScopeAdminService
{
    private static readonly HashSet<string> RootRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Administrator",
        "TenantAdmin",
    };

    public async Task<AccountMeResponseDto> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var (userId, principal) = RequirePrincipal();
        var grantedSet = new HashSet<string>(
            await permissions.GetPermissionsAsync(userId, cancellationToken).ConfigureAwait(false),
            StringComparer.Ordinal);
        ExpandWildcard(grantedSet);

        var user = await realmUsers.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var roles = await ResolveRolesAsync(principal, user, cancellationToken).ConfigureAwait(false);
        var account = await BuildAccountAsync(userId, principal, user, cancellationToken).ConfigureAwait(false);
        var permissionCodes = grantedSet.OrderBy(code => code, StringComparer.Ordinal).ToArray();

        var anchor = await ResolveAnchorUnitAsync(userId, principal, cancellationToken).ConfigureAwait(false);
        if (anchor is null)
        {
            return new AccountMeResponseDto(permissionCodes, roles, null, [], account);
        }

        var descendants = await structure.GetDescendantsAsync(anchor.Id, cancellationToken).ConfigureAwait(false);
        var units = new[] { anchor }
            .Concat(descendants)
            .OrderBy(unit => unit.Path, StringComparer.Ordinal)
            .Select(ToDto)
            .ToArray();
        return new AccountMeResponseDto(permissionCodes, roles, ToDto(anchor), units, account);
    }

    public async Task<UserOrgScopeUpdateOutcome> SetUserScopeAsync(
        Guid userId,
        Guid? organizationalUnitId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        var orgId = await currentOrg.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null || tenantId == Guid.Empty || orgId is null || orgId == Guid.Empty)
            return new UserOrgScopeUpdateOutcome(UserOrgScopeUpdateStatus.ScopeMissing);

        var user = await realmUsers.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            return new UserOrgScopeUpdateOutcome(UserOrgScopeUpdateStatus.NotFound);

        string? unitType = null;
        string? path = null;
        string? legalEntityId = null;
        Guid? resolvedUnitId = null;
        if (organizationalUnitId is Guid unitId && unitId != Guid.Empty)
        {
            var units = await structure.ListUnitsAsync(
                new ListOrganizationalUnitsQuery(orgId.Value, null, null, null),
                cancellationToken).ConfigureAwait(false);
            var unit = units.FirstOrDefault(item => item.Id == unitId);
            if (unit is null)
                return new UserOrgScopeUpdateOutcome(UserOrgScopeUpdateStatus.NotFound);
            unitType = unit.UnitType;
            path = unit.Path;
            legalEntityId = unit.LegalEntityId?.ToString();
            resolvedUnitId = unit.Id;
        }

        await realmUsers.SyncOrgClaimsAsync(
                user,
                resolvedUnitId ?? Guid.Empty,
                unitType ?? string.Empty,
                legalEntityId,
                path ?? string.Empty,
                Guid.Empty,
                cancellationToken)
            .ConfigureAwait(false);

        return new UserOrgScopeUpdateOutcome(
            UserOrgScopeUpdateStatus.Updated,
            new UserOrgScopeResultDto(
                user.Id,
                user.OrganizationalUnitId,
                user.UnitType,
                legalEntityId,
                path));
    }

    /// <summary>Đơn vị neo của user: claim OrganizationalUnitId trước, cột trên user sau.</summary>
    public async Task<OrganizationalUnitDescriptor?> ResolveAnchorUnitAsync(
        Guid userId,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        Guid? unitId = null;
        if (Guid.TryParse(principal.FindFirstValue("OrganizationalUnitId"), out var claimUnitId)
            && claimUnitId != Guid.Empty)
        {
            unitId = claimUnitId;
        }
        else
        {
            var user = await realmUsers.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);
            if (user?.OrganizationalUnitId is Guid columnId && columnId != Guid.Empty)
                unitId = columnId;
        }

        if (unitId is not Guid resolved || resolved == Guid.Empty)
            return null;

        try
        {
            return await structure.GetUnitAsync(resolved, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    private async Task<IReadOnlyList<string>> ResolveRolesAsync(
        ClaimsPrincipal principal,
        Platform.Authentication.Identity.Domain.User? user,
        CancellationToken cancellationToken)
    {
        var fromToken = principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        if (fromToken.Length > 0)
            return fromToken;

        if (user is null)
            return fromToken;

        return await realmUsers.GetRoleNamesAsync(user, cancellationToken).ConfigureAwait(false);
    }

    private async Task<AccountMeUserDto> BuildAccountAsync(
        Guid userId,
        ClaimsPrincipal principal,
        Platform.Authentication.Identity.Domain.User? user,
        CancellationToken cancellationToken)
    {
        string? displayName = null;
        string? email = null;
        var userName = BlankToNull(user?.UserName);
        if (user is not null)
        {
            email = BlankToNull(user.Email);
            var claimName = await realmUsers.GetDisplayNameAsync(user, cancellationToken).ConfigureAwait(false);
            displayName = BlankToNull(claimName);
        }

        userName ??= BlankToNull(principal.FindFirstValue(ClaimTypes.Name)) ?? string.Empty;
        return new AccountMeUserDto(userId, userName, displayName, email);
    }

    private (Guid UserId, ClaimsPrincipal Principal) RequirePrincipal()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("Chưa đăng nhập.");

        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(raw, out var userId))
            throw new UnauthorizedAccessException("Token không hợp lệ.");

        return (userId, principal);
    }

    private void ExpandWildcard(HashSet<string> granted)
    {
        if (!granted.Contains(PermissionCatalog.Wildcard)
            && !granted.Contains(PermissionCatalog.SystemAll))
            return;

        foreach (var descriptor in permissionCatalog.GetAllPermissions())
            granted.Add(descriptor.Code);
    }

    private static AccountMeOrgUnitDto ToDto(OrganizationalUnitDescriptor unit) =>
        new(unit.Id, unit.Code, unit.Name, unit.UnitType, unit.Path);

    private static string? BlankToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }
}
