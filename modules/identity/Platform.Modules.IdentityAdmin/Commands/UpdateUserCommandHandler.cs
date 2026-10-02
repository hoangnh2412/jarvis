namespace Platform.Modules.IdentityAdmin.Commands;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authentication;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;
using Platform.Authorization.Abstractions;

public sealed class UpdateUserCommandHandler : IAsyncCommandHandler<UpdateUserCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;

    public UpdateUserCommandHandler(
        UserManager<User> userManager,
        ICurrentUser<CurrentUserInfo> currentUser,
        IRoleGovernanceValidator roleGovernanceValidator)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _roleGovernanceValidator = roleGovernanceValidator ?? throw new ArgumentNullException(nameof(roleGovernanceValidator));
    }

    public async Task HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
            throw new InvalidOperationException($"User {command.UserId} not found");

        if (!await _roleGovernanceValidator.CanManageUserAsync(actor.UserId.Value, user.Id, cancellationToken))
            throw new UnauthorizedAccessException("You cannot manage a user whose role level is greater than or equal to your own");

        if (command.IsLocked)
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
    }
}
