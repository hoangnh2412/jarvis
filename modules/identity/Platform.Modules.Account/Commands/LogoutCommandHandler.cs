namespace Platform.Modules.Account.Commands;

using Platform.Authentication;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;

public sealed class LogoutCommandHandler : IAsyncCommandHandler<LogoutCommand>
{
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;

    public LogoutCommandHandler(ICurrentUser<CurrentUserInfo> currentUser)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task HandleAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");
    }
}
