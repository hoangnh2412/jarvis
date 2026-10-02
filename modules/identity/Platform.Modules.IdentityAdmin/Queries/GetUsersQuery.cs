namespace Platform.Modules.IdentityAdmin.Queries;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Liệt kê toàn bộ user. Chưa filter theo scope/tenant (data-scope ABAC filtering để Phase 4).
/// </summary>
public sealed record GetUsersQuery : IQuery;

public sealed record UserSummaryDto
{
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
    public required bool IsLocked { get; init; }
    public required IReadOnlyCollection<string> Roles { get; init; }
}
