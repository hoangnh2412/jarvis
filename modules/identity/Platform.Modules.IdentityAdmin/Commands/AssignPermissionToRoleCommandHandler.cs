namespace Platform.Modules.IdentityAdmin.Commands;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;
using Platform.Authorization.Abstractions;
using Platform.Authentication.Identity.Domain;

public sealed class AssignPermissionToRoleCommandHandler : IAsyncCommandHandler<AssignPermissionToRoleCommand>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IPermissionProvider _permissionProvider;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;

    public AssignPermissionToRoleCommandHandler(
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

    public async Task HandleAsync(AssignPermissionToRoleCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var actorHasPermission = await _permissionProvider.HasPermissionAsync(actor.UserId.Value, command.Permission, cancellationToken);
        if (!actorHasPermission)
            throw new UnauthorizedAccessException($"You do not have permission '{command.Permission}' to grant it");

        var role = await _roleManager.FindByIdAsync(command.RoleId.ToString());
        if (role == null)
            throw new InvalidOperationException($"Role {command.RoleId} not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("Cannot modify permissions on a system role");

        if (!await _roleGovernanceValidator.CanManageRoleAsync(actor.UserId.Value, role.Id, cancellationToken))
            throw new UnauthorizedAccessException("You cannot modify a role with level greater than or equal to your own");

        var existingClaim = (await _roleManager.GetClaimsAsync(role))
            .FirstOrDefault(c => c.Type == "Permission" && c.Value == command.Permission);

        if (existingClaim != null)
            return;

        var claim = new Claim("Permission", command.Permission);
        var result = await _roleManager.AddClaimAsync(role, claim);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to assign permission: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
