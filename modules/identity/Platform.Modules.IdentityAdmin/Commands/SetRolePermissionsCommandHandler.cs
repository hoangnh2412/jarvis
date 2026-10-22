namespace Platform.Modules.IdentityAdmin.Commands;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.Authentication.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;

public sealed class SetRolePermissionsCommandHandler : IAsyncCommandHandler<SetRolePermissionsCommand>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IPermissionProvider _permissionProvider;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;

    public SetRolePermissionsCommandHandler(
        RoleManager<Role> roleManager,
        ICurrentUser<CurrentUserInfo> currentUser,
        IPermissionProvider permissionProvider,
        IRoleGovernanceValidator roleGovernanceValidator)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _permissionProvider = permissionProvider ?? throw new ArgumentNullException(nameof(permissionProvider));
        _roleGovernanceValidator = roleGovernanceValidator ?? throw new ArgumentNullException(nameof(roleGovernanceValidator));
    }

    public async Task HandleAsync(SetRolePermissionsCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var role = await _roleManager.FindByIdAsync(command.RoleId.ToString());
        if (role == null)
            throw new InvalidOperationException($"Role {command.RoleId} not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("Cannot modify permissions on a system role");

        if (!await _roleGovernanceValidator.CanManageRoleAsync(actor.UserId.Value, role.Id, cancellationToken))
            throw new UnauthorizedAccessException("You cannot modify a role with level greater than or equal to your own");

        var desired = (command.Permissions ?? [])
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Select(permission => permission.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var existing = (await _roleManager.GetClaimsAsync(role))
            .Where(claim => claim.Type == IdentityClaimTypes.Permission)
            .ToList();
        var existingValues = new HashSet<string>(existing.Select(claim => claim.Value), StringComparer.Ordinal);

        foreach (var permission in desired)
        {
            if (existingValues.Contains(permission))
                continue;

            var actorHasPermission = await _permissionProvider.HasPermissionAsync(actor.UserId.Value, permission, cancellationToken);
            if (!actorHasPermission)
                throw new UnauthorizedAccessException($"You do not have permission '{permission}' to grant it");
        }

        foreach (var claim in existing)
        {
            if (desired.Contains(claim.Value, StringComparer.Ordinal))
                continue;

            var removed = await _roleManager.RemoveClaimAsync(role, claim);
            if (!removed.Succeeded)
                throw new InvalidOperationException($"Failed to assign permission: {string.Join("; ", removed.Errors.Select(error => error.Description))}");
        }

        foreach (var permission in desired)
        {
            if (existingValues.Contains(permission))
                continue;

            var added = await _roleManager.AddClaimAsync(role, new Claim(IdentityClaimTypes.Permission, permission));
            if (!added.Succeeded)
                throw new InvalidOperationException($"Failed to assign permission: {string.Join("; ", added.Errors.Select(error => error.Description))}");
        }
    }
}
