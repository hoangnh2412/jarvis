namespace Platform.Modules.IdentityAdmin.Commands;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;

public sealed class DeleteRoleCommandHandler : IAsyncCommandHandler<DeleteRoleCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;

    public DeleteRoleCommandHandler(
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

    public async Task HandleAsync(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var role = await _roleManager.FindByIdAsync(command.RoleId.ToString());
        if (role == null)
            throw new InvalidOperationException($"Role {command.RoleId} not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("Cannot delete a system role");

        if (!await _roleGovernanceValidator.CanManageRoleAsync(actor.UserId.Value, role.Id, cancellationToken))
            throw new UnauthorizedAccessException("You cannot modify a role with level greater than or equal to your own");

        if (!string.IsNullOrEmpty(role.Name))
        {
            var members = await _userManager.GetUsersInRoleAsync(role.Name);
            if (members.Count > 0)
                throw new InvalidOperationException("Cannot delete a role that is still assigned to users");
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to delete role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
    }
}
