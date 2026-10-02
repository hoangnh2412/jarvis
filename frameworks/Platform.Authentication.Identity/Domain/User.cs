namespace Platform.Authentication.Identity.Domain;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Application user extending ASP.NET Identity user.
/// TenantId / OrgId are columns on AspNetUsers (ADR RBAC+ABAC §7 Q3); other subject attributes
/// (DepartmentId, ApproveLimit, …) stay in AspNetUserClaims.
/// </summary>
public sealed class User : IdentityUser<Guid>
{
    /// <summary>
    /// Tenant the user belongs to; null = global (system Admin).
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Organization (member company) inside the tenant; null when the user is tenant-wide.
    /// </summary>
    public Guid? OrgId { get; set; }
}
