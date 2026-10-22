namespace Platform.Orgs.Hooks;

/// <summary>Context chung cho hook Error / Finally (thành công hoặc lỗi).</summary>
public abstract class OrgCatalogHookContextBase
{
    public Guid TenantId { get; init; }

    /// <summary>True sau khi <c>SaveChanges</c> thành công.</summary>
    public bool Succeeded { get; set; }

    /// <summary>Lỗi phát sinh trong pipeline (nếu có).</summary>
    public Exception? Exception { get; set; }
}
