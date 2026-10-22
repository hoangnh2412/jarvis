namespace Platform.Orgs.Abstractions.Events;

public sealed record OrgCreatedEvent(
    Guid TenantId,
    Guid OrgId,
    DateTimeOffset OccurredAt,
    OrgDescriptor Snapshot)
    : OrgCatalogEventBase(TenantId, OrgId, OccurredAt);
