namespace Platform.Modules.IdentityAdmin.Commands;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authentication;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;
using Platform.Authorization.Abstractions;

public sealed class DeleteUserCommandHandler : IAsyncCommandHandler<DeleteUserCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;

    public DeleteUserCommandHandler(
        UserManager<User> userManager,
        ICurrentUser<CurrentUserInfo> currentUser,
        IRoleGovernanceValidator roleGovernanceValidator)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _roleGovernanceValidator = roleGovernanceValidator ?? throw new ArgumentNullException(nameof(roleGovernanceValidator));
    }

    public async Task HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        if (actor.UserId.Value == command.UserId)
            throw new InvalidOperationException("Cannot delete your own account");

        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
            throw new InvalidOperationException($"User {command.UserId} not found");

        if (!await _roleGovernanceValidator.CanManageUserAsync(actor.UserId.Value, user.Id, cancellationToken))
            throw new UnauthorizedAccessException("You cannot manage a user whose role level is greater than or equal to your own");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to delete user: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
