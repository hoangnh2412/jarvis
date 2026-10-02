namespace Platform.Modules.Account.Commands;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authentication;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;

public sealed class ChangePasswordCommandHandler : IAsyncCommandHandler<ChangePasswordCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;

    public ChangePasswordCommandHandler(
        UserManager<User> userManager,
        ICurrentUser<CurrentUserInfo> currentUser)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var user = await _userManager.FindByIdAsync(actor.UserId.Value.ToString());
        if (user == null)
            throw new InvalidOperationException("User not found");

        var result = await _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to change password: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
