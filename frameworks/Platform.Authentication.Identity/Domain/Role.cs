namespace Platform.Authentication.Identity.Domain;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Application role extending ASP.NET Identity role with authorization-specific fields.
/// </summary>
public sealed class Role : IdentityRole<Guid>
{
    /// <summary>
    /// Role governance level: System roles cannot be modified by non-Admin.
    /// Higher level actor can only assign/modify roles with lower level.
    /// Range: 1–1000 (1000=Admin, 900=TenantAdmin, 800=OrgAdmin, 100=Employee)
    /// </summary>
    public int RoleLevel { get; set; } = 100;

    /// <summary>
    /// System roles (Admin, TenantAdmin, OrgAdmin) are immutable and cannot be deleted.
    /// </summary>
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// Display name for UI; differs from normalized Name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Timestamp for audit.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
