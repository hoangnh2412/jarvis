namespace Platform.Modules.IdentityAdmin.Commands;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;

public sealed class CreateRoleCommandHandler : IAsyncCommandHandler<CreateRoleCommand, CreateRoleResponse>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;

    public CreateRoleCommandHandler(
        RoleManager<Role> roleManager,
        ICurrentUser<CurrentUserInfo> currentUser,
        IRoleGovernanceValidator roleGovernanceValidator)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _roleGovernanceValidator = roleGovernanceValidator ?? throw new ArgumentNullException(nameof(roleGovernanceValidator));
    }

    public async Task<CreateRoleResponse> HandleAsync(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        if (!await _roleGovernanceValidator.CanCreateRoleWithLevelAsync(actor.UserId.Value, command.RoleLevel, cancellationToken))
            throw new UnauthorizedAccessException("You cannot create a role with level greater than or equal to your own");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            DisplayName = command.DisplayName,
            RoleLevel = command.RoleLevel,
            IsSystemRole = false
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create role: {string.Join("; ", result.Errors.Select(e => e.Description))}");

        return new CreateRoleResponse
        {
            RoleId = role.Id,
            Name = role.Name ?? command.Name
        };
    }
}
