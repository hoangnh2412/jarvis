namespace Platform.Modules.Account.Queries;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authentication;
using Platform.DDD.Application.Contracts.Queries;
using Platform.DDD.Domain.Services;

public sealed class GetMyProfileQueryHandler : IAsyncQueryHandler<GetMyProfileQuery, MyProfileResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;

    public GetMyProfileQueryHandler(
        UserManager<User> userManager,
        ICurrentUser<CurrentUserInfo> currentUser)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<MyProfileResponse> HandleAsync(GetMyProfileQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var user = await _userManager.FindByIdAsync(actor.UserId.Value.ToString());
        if (user == null)
            throw new InvalidOperationException("User not found");

        return new MyProfileResponse
        {
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
    }
}
