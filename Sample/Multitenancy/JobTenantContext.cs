namespace Sample.Multitenancy;

/// <summary>
/// Ambient tenant id for background jobs (no HTTP). Set before creating UoW/DbContext in a new scope.
/// </summary>
public sealed class JobTenantContext
{
    public Guid? TenantId { get; set; }
}
