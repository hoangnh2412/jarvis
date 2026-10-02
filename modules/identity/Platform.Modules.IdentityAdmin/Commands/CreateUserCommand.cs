namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Create new user with role assignment.
/// Enforces Role Governance: actor RoleLevel must be > target role level.
/// </summary>
public sealed record CreateUserCommand : ICommand
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required Guid RoleId { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? OrgId { get; init; }
}

public sealed record CreateUserResponse
{
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
}
