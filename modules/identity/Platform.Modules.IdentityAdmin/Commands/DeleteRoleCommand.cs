namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

public sealed record DeleteRoleCommand : ICommand
{
    public required Guid RoleId { get; init; }
}
