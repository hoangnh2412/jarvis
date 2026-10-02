namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

public sealed record DeleteUserCommand : ICommand
{
    public required Guid UserId { get; init; }
}
