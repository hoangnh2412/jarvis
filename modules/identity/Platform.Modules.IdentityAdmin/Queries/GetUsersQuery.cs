namespace Platform.Modules.IdentityAdmin.Queries;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Liệt kê user trong scope của actor: global thấy mọi tenant, tenant admin thấy tenant mình, org admin thấy org mình.
/// </summary>
public sealed record GetUsersQuery : IQuery;

public sealed record UserSummaryDto
{
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? DisplayName { get; init; }
    public required bool IsLocked { get; init; }
    public required IReadOnlyCollection<string> Roles { get; init; }
    public IReadOnlyCollection<Guid> RoleIds { get; init; } = [];
}
