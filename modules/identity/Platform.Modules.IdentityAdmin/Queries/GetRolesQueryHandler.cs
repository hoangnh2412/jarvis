namespace Platform.Modules.IdentityAdmin.Queries;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.Authentication.Identity.Domain;
using Platform.DDD.Application.Contracts.Queries;
using Platform.DDD.Domain.Services;

public sealed class GetRolesQueryHandler : IAsyncQueryHandler<GetRolesQuery, IReadOnlyCollection<RoleSummaryDto>>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;

    public GetRolesQueryHandler(RoleManager<Role> roleManager, ICurrentUser<CurrentUserInfo> currentUser)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<IReadOnlyCollection<RoleSummaryDto>> HandleAsync(GetRolesQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var roles = await _roleManager.Roles.MaterializeAsync(cancellationToken);
        var result = new List<RoleSummaryDto>(roles.Count);
        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            result.Add(new RoleSummaryDto
            {
                RoleId = role.Id,
                Name = role.Name ?? string.Empty,
                DisplayName = role.DisplayName,
                RoleLevel = role.RoleLevel,
                IsSystemRole = role.IsSystemRole,
                IsActive = IdentityAdminReader.IsActive(claims),
                Permissions = IdentityAdminReader.Permissions(claims)
            });
        }

        return result;
    }
}
