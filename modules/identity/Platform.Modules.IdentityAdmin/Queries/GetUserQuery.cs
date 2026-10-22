namespace Platform.Modules.IdentityAdmin.Queries;

using Platform.DDD.Domain.Shared.Messaging;

public sealed record GetUserQuery : IQuery
{
    public required Guid UserId { get; init; }
}
