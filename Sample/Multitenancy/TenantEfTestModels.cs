namespace Sample.Multitenancy;

public sealed record TenantEfTestResult(
    string Scenario,
    Guid? ResolvedTenantId,
    string? ConnectionStringPreview,
    string? Database,
    int StudentCount,
    string? Hint);

public sealed record BackgroundJobTenantRequest(Guid TenantId);
