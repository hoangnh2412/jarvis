namespace Platform.Modules.IdentityAdmin.Queries;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;
using Platform.DDD.Application.Contracts.Queries;
using Platform.DDD.Domain.Services;

public sealed class GetUserQueryHandler : IAsyncQueryHandler<GetUserQuery, UserDetailDto>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;

    public GetUserQueryHandler(
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

    public async Task<UserDetailDto> HandleAsync(GetUserQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var user = await _userManager.FindByIdAsync(query.UserId.ToString());
        if (user == null)
            throw new InvalidOperationException($"User {query.UserId} not found");

        if (!await _roleGovernanceValidator.IsWithinActorScopeAsync(actor.UserId.Value, user.TenantId, user.OrgId, cancellationToken))
            throw new UnauthorizedAccessException("You cannot manage a user outside your tenant or organization");

        var roleNames = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var roleIds = new List<Guid>(roleNames.Count);
        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
                roleIds.Add(role.Id);
        }

        return new UserDetailDto
        {
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DisplayName = IdentityAdminReader.DisplayName(claims),
            IsLocked = await _userManager.IsLockedOutAsync(user),
            RoleIds = roleIds,
            Roles = roleNames.ToList(),
            TenantId = user.TenantId,
            OrgId = user.OrgId
        };
    }
}
