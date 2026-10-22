namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Replaces profile, lock state, and the full role set of a user.
/// An empty <see cref="Password"/> leaves the current password unchanged.
/// </summary>
public sealed record UpdateUserAccountCommand : ICommand
{
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? DisplayName { get; init; }
    public string? Password { get; init; }
    public required bool IsLocked { get; init; }
    public required IReadOnlyCollection<Guid> RoleIds { get; init; }
}
