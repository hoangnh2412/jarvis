namespace Platform.Modules.IdentityAdmin.Commands;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;

public sealed class CreateUserCommandHandler : IAsyncCommandHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;

    public CreateUserCommandHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IRoleGovernanceValidator roleGovernanceValidator,
        ICurrentUser<CurrentUserInfo> currentUser)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _roleGovernanceValidator = roleGovernanceValidator ?? throw new ArgumentNullException(nameof(roleGovernanceValidator));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<CreateUserResponse> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var targetRole = await _roleManager.FindByIdAsync(command.RoleId.ToString());
        if (targetRole == null)
            throw new InvalidOperationException($"Role {command.RoleId} not found");

        var canAssign = await _roleGovernanceValidator.CanAssignRoleAsync(actor.UserId.Value, command.RoleId, cancellationToken);
        if (!canAssign)
            throw new UnauthorizedAccessException("You do not have permission to assign this role");

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = command.Username,
            Email = command.Email,
            NormalizedUserName = command.Username.ToUpperInvariant(),
            NormalizedEmail = command.Email.ToUpperInvariant(),
            TenantId = command.TenantId,
            OrgId = command.OrgId
        };

        var createResult = await _userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException($"Failed to create user: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");

        var addRoleResult = await _userManager.AddToRoleAsync(user, targetRole.Name ?? string.Empty);
        if (!addRoleResult.Succeeded)
            throw new InvalidOperationException($"Failed to assign role: {string.Join("; ", addRoleResult.Errors.Select(e => e.Description))}");

        return new CreateUserResponse
        {
            UserId = user.Id,
            Username = user.UserName!
        };
    }
}
