namespace Platform.Authorization.EntityFramework.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Domain;
using Platform.Authorization.EntityFramework.Persistence.EntityConfigurations;

/// <summary>
/// DbContext cơ sở cho các mô hình ủy quyền.
/// Mở rộng với các thuộc tính DbSet cho Role, Policy, PolicyRule.
/// Sản phẩm nên kế thừa từ cái này và cấu hình các thực thể bổ sung.
/// </summary>
public abstract class IdentityDbContextBase : DbContext
{
    public IdentityDbContextBase(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Các thực thể vai trò (mở rộng IdentityRole).
    /// </summary>
    public virtual DbSet<Role> Roles => Set<Role>();

    /// <summary>
    /// Người dùng (các cột TenantId / OrgId là các thuộc tính chủ thể ABAC — ADR Q3).
    /// </summary>
    public virtual DbSet<User> Users => Set<User>();

    /// <summary>
    /// User claims giữ các thuộc tính chủ thể còn lại (DepartmentId, ApproveLimit, …).
    /// </summary>
    public virtual DbSet<IdentityUserClaim<Guid>> UserClaims => Set<IdentityUserClaim<Guid>>();

    /// <summary>
    /// Các chính sách ủy quyền (ABAC).
    /// </summary>
    public virtual DbSet<Policy> Policies => Set<Policy>();

    /// <summary>
    /// Các quy tắc chính sách (các biểu thức điều kiện).
    /// </summary>
    public virtual DbSet<PolicyRule> PolicyRules => Set<PolicyRule>();

    private DbSet<IdentityUserRole<Guid>>? _userRoles;
    private DbSet<IdentityRoleClaim<Guid>>? _roleClaims;

    /// <summary>
    /// Các liên kết User-Role (để truy vấn các vai trò của người dùng).
    /// </summary>
    public virtual DbSet<IdentityUserRole<Guid>> UserRoles
    {
        get => _userRoles ??= Set<IdentityUserRole<Guid>>();
    }

    /// <summary>
    /// Role-Claim associations (for querying role's claims like Permissions, Policies).
    /// </summary>
    public virtual DbSet<IdentityRoleClaim<Guid>> RoleClaims
    {
        get => _roleClaims ??= Set<IdentityRoleClaim<Guid>>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Shared ASP.NET Identity store (ADR C14): same tables as IdentityDbContext<User, Role, Guid>.
        modelBuilder.Entity<User>().ToTable("AspNetUsers");

        modelBuilder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("AspNetUserRoles");
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
        });

        modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("AspNetRoleClaims");
            entity.HasKey(rc => rc.Id);
        });

        modelBuilder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("AspNetUserClaims");
            entity.HasKey(c => c.Id);
        });

        // Apply configurations
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PolicyConfiguration());
        modelBuilder.ApplyConfiguration(new PolicyRuleConfiguration());
    }
}
