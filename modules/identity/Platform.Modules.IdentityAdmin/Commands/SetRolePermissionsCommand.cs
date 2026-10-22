namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Replaces the permission claims of a role. Does not create or delete permission definitions.
/// Granting a code the actor does not hold is rejected. Revoking does not require that code.
/// </summary>
public sealed record SetRolePermissionsCommand : ICommand
{
    public required Guid RoleId { get; init; }
    public required IReadOnlyCollection<string> Permissions { get; init; }
}
