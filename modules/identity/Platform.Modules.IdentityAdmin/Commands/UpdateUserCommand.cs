namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Lock/unlock user (CRUD User theo ADR §11.3). IsLocked = true khóa vô thời hạn,
/// false mở khóa ngay.
/// </summary>
public sealed record UpdateUserCommand : ICommand
{
    public required Guid UserId { get; init; }
    public required bool IsLocked { get; init; }
}
