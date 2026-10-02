namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Assign a permission to a role.
/// Enforces scope check: actor must have the permission being granted.
/// </summary>
public sealed record AssignPermissionToRoleCommand : ICommand
{
    public required Guid RoleId { get; init; }
    public required string Permission { get; init; }
}
