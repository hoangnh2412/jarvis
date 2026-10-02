namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

public sealed record RemovePermissionFromRoleCommand : ICommand
{
    public required Guid RoleId { get; init; }
    public required string Permission { get; init; }
}
