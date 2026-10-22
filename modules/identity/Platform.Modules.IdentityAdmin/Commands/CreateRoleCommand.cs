namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Create a new custom (non-system) role.
/// Enforces Role Governance: actor's max RoleLevel must be > RoleLevel của role mới.
/// </summary>
public sealed record CreateRoleCommand : ICommand
{
    public required string Name { get; init; }
    public string? DisplayName { get; init; }
    public required int RoleLevel { get; init; }

    /// <summary>False stores a <c>role_active=false</c> claim. Omitted means active.</summary>
    public bool IsActive { get; init; } = true;
}

public sealed record CreateRoleResponse
{
    public required Guid RoleId { get; init; }
    public required string Name { get; init; }
}
