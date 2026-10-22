namespace Platform.Modules.IdentityAdmin.Queries;

using Platform.DDD.Domain.Shared.Messaging;

public sealed record GetRoleQuery : IQuery
{
    public required Guid RoleId { get; init; }
}
