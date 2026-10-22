namespace Platform.Orgs.Abstractions.Events;

public sealed record OrgUpdatedEvent(
    Guid TenantId,
    Guid OrgId,
    DateTimeOffset OccurredAt,
    OrgDescriptor Snapshot)
    : OrgCatalogEventBase(TenantId, OrgId, OccurredAt);
