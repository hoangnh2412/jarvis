namespace Platform.Authentication.Identity.Domain;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Application user extending ASP.NET Identity user.
/// <c>TenantId</c> / <c>OrgId</c> are scope columns on AspNetUsers; other subject attributes
/// (DepartmentId, ApproveLimit, …) live in AspNetUserClaims (ADR RBAC+ABAC §5.2).
/// </summary>
public sealed class User : IdentityUser<Guid>
{
    /// <summary>
    /// Tenant the user belongs to; null = global (system Admin).
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>Đơn vị trên cây tổ chức. Null khi user chưa được gán.</summary>
    public Guid? OrganizationalUnitId { get; set; }

    /// <summary>Loại đơn vị tương ứng <see cref="OrganizationalUnitId"/>.</summary>
    public string? UnitType { get; set; }

    /// <summary>
    /// Organization (member company) inside the tenant; null when the user is tenant-wide.
    /// </summary>
    public Guid? OrgId { get; set; }
}
