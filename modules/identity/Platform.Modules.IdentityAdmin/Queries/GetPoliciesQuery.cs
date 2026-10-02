namespace Platform.Modules.IdentityAdmin.Queries;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Trả về tập policy mà actor hiện tại đang có (grantable set), union qua các role của actor.
/// </summary>
public sealed record GetPoliciesQuery : IQuery;

public sealed record PolicySummaryDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? ResourceType { get; init; }
    public string? Action { get; init; }
}
