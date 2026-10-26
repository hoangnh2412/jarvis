namespace Platform.Orgs.Abstractions.Events;

/// <summary>Một field chuẩn đã đổi. Không gửi cả bản ghi.</summary>
public sealed record OrgFieldChange(string Name, string? Before, string? After);

public sealed record OrgUpdatedEvent(
    Guid TenantId,
    Guid OrgId,
    DateTimeOffset OccurredAt,
    IReadOnlyList<OrgFieldChange> Changes)
    : OrgCatalogEventBase(TenantId, OrgId, OccurredAt);
