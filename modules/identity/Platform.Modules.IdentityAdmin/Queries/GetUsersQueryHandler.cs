namespace Platform.Modules.IdentityAdmin.Queries;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.Authentication.Identity.Domain;
using Platform.DDD.Application.Contracts.Queries;
using Platform.DDD.Domain.Services;

public sealed class GetUsersQueryHandler : IAsyncQueryHandler<GetUsersQuery, IReadOnlyCollection<UserSummaryDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;

    public GetUsersQueryHandler(UserManager<User> userManager, ICurrentUser<CurrentUserInfo> currentUser)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<IReadOnlyCollection<UserSummaryDto>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var actorUser = await _userManager.FindByIdAsync(actor.UserId.Value.ToString());
        if (actorUser == null)
            throw new UnauthorizedAccessException("Current user not found");

        IQueryable<User> source = _userManager.Users;
        if (actorUser.TenantId is { } tenantId)
            source = source.Where(u => u.TenantId == tenantId);
        if (actorUser.OrgId is { } orgId)
            source = source.Where(u => u.OrgId == orgId);

        var users = await source.MaterializeAsync(cancellationToken);

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
