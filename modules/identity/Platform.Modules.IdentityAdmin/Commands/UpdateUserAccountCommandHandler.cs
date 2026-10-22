namespace Platform.Modules.IdentityAdmin.Commands;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.Authentication.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;

public sealed class UpdateUserAccountCommandHandler : IAsyncCommandHandler<UpdateUserAccountCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;

    public UpdateUserAccountCommandHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ICurrentUser<CurrentUserInfo> currentUser,
        IRoleGovernanceValidator roleGovernanceValidator)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _roleGovernanceValidator = roleGovernanceValidator ?? throw new ArgumentNullException(nameof(roleGovernanceValidator));
    }

    public async Task HandleAsync(UpdateUserAccountCommand command, CancellationToken cancellationToken)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
            throw new InvalidOperationException($"User {command.UserId} not found");

        if (!await _roleGovernanceValidator.CanManageUserAsync(actor, user.Id, cancellationToken))
            throw new UnauthorizedAccessException("You cannot manage a user whose role level is greater than or equal to your own");

        if (!await _roleGovernanceValidator.IsWithinActorScopeAsync(actor, user.TenantId, user.OrgId, cancellationToken))
            throw new UnauthorizedAccessException("You cannot manage a user outside your tenant or organization");

        var desiredRoles = await LoadRolesAsync(actor, command.RoleIds, cancellationToken);
        await ReplaceRolesAsync(actor, user, desiredRoles, cancellationToken);
        await UpdateProfileAsync(user, command);
        await UpdatePasswordAsync(user, command.Password);
        await UpdateLockAsync(user, command.IsLocked);
    }

    private async Task<Guid> RequireActorAsync(CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        return actor.UserId.Value;
    }

    private async Task<List<Role>> LoadRolesAsync(Guid actorUserId, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken)
    {
        var distinctIds = roleIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (distinctIds.Count == 0)
            throw new InvalidOperationException("At least one role is required");

        var roles = new List<Role>(distinctIds.Count);
        foreach (var roleId in distinctIds)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
                throw new InvalidOperationException($"Role {roleId} not found");

            if (!await _roleGovernanceValidator.CanAssignRoleAsync(actorUserId, roleId, cancellationToken))
                throw new UnauthorizedAccessException("You do not have permission to assign this role");

            roles.Add(role);
        }

        return roles;
    }

    private async Task ReplaceRolesAsync(Guid actorUserId, User user, IReadOnlyList<Role> desiredRoles, CancellationToken cancellationToken)
    {
        var currentNames = await _userManager.GetRolesAsync(user);
        var desiredNames = new HashSet<string>(
            desiredRoles.Select(role => role.Name ?? string.Empty),
            StringComparer.OrdinalIgnoreCase);

        foreach (var currentName in currentNames)
        {
            if (desiredNames.Contains(currentName))
                continue;

            var currentRole = await _roleManager.FindByNameAsync(currentName);
            if (currentRole != null && !await _roleGovernanceValidator.CanManageRoleAsync(actorUserId, currentRole.Id, cancellationToken))
                throw new UnauthorizedAccessException("You cannot remove a role with level greater than or equal to your own");

            var removed = await _userManager.RemoveFromRoleAsync(user, currentName);
            if (!removed.Succeeded)
                throw new InvalidOperationException($"Failed to remove role: {string.Join("; ", removed.Errors.Select(error => error.Description))}");
        }

        foreach (var role in desiredRoles)
        {
            var roleName = role.Name ?? string.Empty;
            if (currentNames.Contains(roleName, StringComparer.OrdinalIgnoreCase))
                continue;

            var added = await _userManager.AddToRoleAsync(user, roleName);
            if (!added.Succeeded)
                throw new InvalidOperationException($"Failed to assign role: {string.Join("; ", added.Errors.Select(error => error.Description))}");
        }
    }

    private async Task UpdateProfileAsync(User user, UpdateUserAccountCommand command)
    {
        var username = command.Username.Trim();
        if (!string.Equals(user.UserName, username, StringComparison.Ordinal))
        {
            var renamed = await _userManager.SetUserNameAsync(user, username);
            if (!renamed.Succeeded)
                throw new InvalidOperationException($"Failed to rename user: {string.Join("; ", renamed.Errors.Select(error => error.Description))}");
        }

        var email = command.Email.Trim();
        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var emailResult = await _userManager.SetEmailAsync(user, email);
            if (!emailResult.Succeeded)
                throw new InvalidOperationException($"Failed to update email: {string.Join("; ", emailResult.Errors.Select(error => error.Description))}");
        }

        user.PhoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber) ? null : command.PhoneNumber.Trim();
        var updated = await _userManager.UpdateAsync(user);
        if (!updated.Succeeded)
            throw new InvalidOperationException($"Failed to update user: {string.Join("; ", updated.Errors.Select(error => error.Description))}");

        await ReplaceDisplayNameAsync(user, command.DisplayName);
    }

    private async Task ReplaceDisplayNameAsync(User user, string? displayName)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var current = claims.FirstOrDefault(claim => claim.Type == IdentityClaimTypes.DisplayName);
        var next = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        if (string.Equals(current?.Value, next, StringComparison.Ordinal))
            return;

        if (current != null)
        {
            var removed = await _userManager.RemoveClaimAsync(user, current);
            if (!removed.Succeeded)
                throw new InvalidOperationException($"Failed to update display name: {string.Join("; ", removed.Errors.Select(error => error.Description))}");
        }

        if (next == null)
            return;

        var added = await _userManager.AddClaimAsync(user, new Claim(IdentityClaimTypes.DisplayName, next));
        if (!added.Succeeded)
            throw new InvalidOperationException($"Failed to update display name: {string.Join("; ", added.Errors.Select(error => error.Description))}");
    }

    private async Task UpdatePasswordAsync(User user, string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return;

        if (await _userManager.HasPasswordAsync(user))
        {
            var removed = await _userManager.RemovePasswordAsync(user);
            if (!removed.Succeeded)
                throw new InvalidOperationException($"Failed to replace password: {string.Join("; ", removed.Errors.Select(error => error.Description))}");
        }

        var added = await _userManager.AddPasswordAsync(user, password);
        if (!added.Succeeded)
            throw new InvalidOperationException($"Failed to replace password: {string.Join("; ", added.Errors.Select(error => error.Description))}");
    }

    private async Task UpdateLockAsync(User user, bool isLocked)
    {
        if (isLocked)
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return;
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
    }
}
