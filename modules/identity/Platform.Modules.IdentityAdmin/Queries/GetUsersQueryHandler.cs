namespace Platform.Modules.IdentityAdmin.Queries;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.DDD.Application.Contracts.Queries;

public sealed class GetUsersQueryHandler : IAsyncQueryHandler<GetUsersQuery, IReadOnlyCollection<UserSummaryDto>>
{
    private readonly UserManager<User> _userManager;

    public GetUsersQueryHandler(UserManager<User> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task<IReadOnlyCollection<UserSummaryDto>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.MaterializeAsync(cancellationToken);

        var result = new List<UserSummaryDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserSummaryDto
            {
                UserId = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                IsLocked = await _userManager.IsLockedOutAsync(user),
                Roles = roles.ToList()
            });
        }

        return result;
    }
}
