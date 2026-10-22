namespace Platform.Orgs.Abstractions.Events;

/// <summary>Metadata chung cho catalog integration events (ADR catalog-events D2).</summary>
public abstract record OrgCatalogEventBase(
    Guid TenantId,
    Guid OrgId,
    DateTimeOffset OccurredAt);
