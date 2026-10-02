namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

public sealed record RemovePolicyFromRoleCommand : ICommand
{
    public required Guid RoleId { get; init; }
    public required string PolicyCode { get; init; }
}
