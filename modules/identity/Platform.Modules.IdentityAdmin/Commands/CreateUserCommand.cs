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

    /// <summary>Single role. Combined with <see cref="RoleIds"/>; an empty id is ignored.</summary>
    public Guid RoleId { get; init; }

    /// <summary>Roles assigned together. At least one id is required across this and <see cref="RoleId"/>.</summary>
    public IReadOnlyCollection<Guid> RoleIds { get; init; } = [];

    public string? PhoneNumber { get; init; }

    public string? DisplayName { get; init; }

    public Guid? TenantId { get; init; }

    public Guid? OrgId { get; init; }
}

public sealed record CreateUserResponse
{
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
}
