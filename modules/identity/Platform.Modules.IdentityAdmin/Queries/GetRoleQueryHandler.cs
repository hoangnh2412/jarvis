namespace Platform.Modules.IdentityAdmin.Queries;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.Authentication.Identity.Domain;
using Platform.DDD.Application.Contracts.Queries;
using Platform.DDD.Domain.Services;

public sealed class GetRoleQueryHandler : IAsyncQueryHandler<GetRoleQuery, RoleDetailDto>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;

    public GetRoleQueryHandler(RoleManager<Role> roleManager, ICurrentUser<CurrentUserInfo> currentUser)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<RoleDetailDto> HandleAsync(GetRoleQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var role = await _roleManager.FindByIdAsync(query.RoleId.ToString());
        if (role == null)
            throw new InvalidOperationException($"Role {query.RoleId} not found");

        var claims = await _roleManager.GetClaimsAsync(role);
        return new RoleDetailDto
        {
            RoleId = role.Id,
            Name = role.Name ?? string.Empty,
            DisplayName = role.DisplayName,
            RoleLevel = role.RoleLevel,
            IsSystemRole = role.IsSystemRole,
            IsActive = IdentityAdminReader.IsActive(claims),
            Permissions = IdentityAdminReader.Permissions(claims),
            Policies = IdentityAdminReader.Policies(claims)
        };
    }
}
