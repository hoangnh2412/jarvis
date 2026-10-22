namespace Platform.Modules.IdentityAdmin.Commands;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.Authentication.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;

public sealed class UpdateRoleCommandHandler : IAsyncCommandHandler<UpdateRoleCommand>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;

    public UpdateRoleCommandHandler(
        RoleManager<Role> roleManager,
        ICurrentUser<CurrentUserInfo> currentUser,
        IRoleGovernanceValidator roleGovernanceValidator)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _roleGovernanceValidator = roleGovernanceValidator ?? throw new ArgumentNullException(nameof(roleGovernanceValidator));
    }

    public async Task HandleAsync(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var role = await _roleManager.FindByIdAsync(command.RoleId.ToString());
        if (role == null)
            throw new InvalidOperationException($"Role {command.RoleId} not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("Cannot modify a system role");

        if (!await _roleGovernanceValidator.CanManageRoleAsync(actor.UserId.Value, role.Id, cancellationToken))
            throw new UnauthorizedAccessException("You cannot modify a role with level greater than or equal to your own");

        if (!await _roleGovernanceValidator.CanCreateRoleWithLevelAsync(actor.UserId.Value, command.RoleLevel, cancellationToken))
            throw new UnauthorizedAccessException("You cannot create a role with level greater than or equal to your own");

        var renamed = await _roleManager.SetRoleNameAsync(role, command.Name.Trim());
        if (!renamed.Succeeded)
            throw new InvalidOperationException($"Failed to update role: {string.Join("; ", renamed.Errors.Select(error => error.Description))}");

        role.DisplayName = string.IsNullOrWhiteSpace(command.DisplayName) ? null : command.DisplayName.Trim();
        role.RoleLevel = command.RoleLevel;
        var updated = await _roleManager.UpdateAsync(role);
        if (!updated.Succeeded)
            throw new InvalidOperationException($"Failed to update role: {string.Join("; ", updated.Errors.Select(error => error.Description))}");

        await SetActiveAsync(role, command.IsActive);
    }

    private async Task SetActiveAsync(Role role, bool isActive)
    {
        var claims = await _roleManager.GetClaimsAsync(role);
        var activeClaim = claims.FirstOrDefault(claim => claim.Type == IdentityClaimTypes.RoleActive);
        if (isActive)
        {
            if (activeClaim == null)
                return;

            var removed = await _roleManager.RemoveClaimAsync(role, activeClaim);
            if (!removed.Succeeded)
                throw new InvalidOperationException($"Failed to update role: {string.Join("; ", removed.Errors.Select(error => error.Description))}");
            return;
        }

        if (activeClaim != null && string.Equals(activeClaim.Value, "false", StringComparison.OrdinalIgnoreCase))
            return;

        if (activeClaim != null)
        {
            var removed = await _roleManager.RemoveClaimAsync(role, activeClaim);
            if (!removed.Succeeded)
                throw new InvalidOperationException($"Failed to update role: {string.Join("; ", removed.Errors.Select(error => error.Description))}");
        }

        var added = await _roleManager.AddClaimAsync(role, new Claim(IdentityClaimTypes.RoleActive, "false"));
        if (!added.Succeeded)
            throw new InvalidOperationException($"Failed to update role: {string.Join("; ", added.Errors.Select(error => error.Description))}");
    }
}
