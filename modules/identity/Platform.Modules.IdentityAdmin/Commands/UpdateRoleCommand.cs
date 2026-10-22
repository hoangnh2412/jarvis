namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Updates a custom role. System roles are rejected.
/// </summary>
public sealed record UpdateRoleCommand : ICommand
{
    public required Guid RoleId { get; init; }
    public required string Name { get; init; }
    public string? DisplayName { get; init; }
    public required int RoleLevel { get; init; }
    public required bool IsActive { get; init; }
}
