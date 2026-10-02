namespace Platform.Modules.IdentityAdmin.Queries;

using Platform.DDD.Domain.Shared.Messaging;

public sealed record GetRolesQuery : IQuery;

public sealed record RoleSummaryDto
{
    public required Guid RoleId { get; init; }
    public required string Name { get; init; }
    public string? DisplayName { get; init; }
    public required int RoleLevel { get; init; }
    public required bool IsSystemRole { get; init; }
}
