using Microsoft.EntityFrameworkCore;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Host.Entities;
using Platform.OrgStructure.EntityFramework.Extensions;

namespace Platform.Host.Postgresql.Persistence;

/// <summary>
/// DbContext tenant mặc định: map bộ entity org-structure chuẩn.
/// Product có bảng riêng thì kế thừa class này. Product đổi shape entity thì không kế thừa.
/// </summary>
public class PlatformTenantDbContext : IdentityDbContextBase
{
    public PlatformTenantDbContext(DbContextOptions<PlatformTenantDbContext> options)
        : base(options)
    {
    }

    protected PlatformTenantDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyPlatformOrgStructure<OrgUnit, LegalEntity, JobTitle, OrgChartPosition>();
        modelBuilder.ApplyPlatformOrgCatalog<OrgUnitType, JobGrade, JobPosition, OrgUnitMember>();
        modelBuilder.Entity<OrgUnit>().ToTable("org_structure_unit");
        modelBuilder.Entity<LegalEntity>(entity =>
        {
            entity.ToTable("org_structure_legal_entity");
            entity.Property(item => item.DetailsJson);
        });
        modelBuilder.Entity<JobTitle>().ToTable("org_structure_job_title");
        modelBuilder.Entity<OrgChartPosition>().ToTable("org_structure_position");
        modelBuilder.Entity<OrgUnitType>().ToTable("org_structure_unit_type");
        modelBuilder.Entity<JobGrade>().ToTable("org_structure_job_grade");
        modelBuilder.Entity<JobPosition>().ToTable("org_structure_job_position");
        modelBuilder.Entity<OrgUnitMember>().ToTable("org_structure_unit_member");
    }
}
