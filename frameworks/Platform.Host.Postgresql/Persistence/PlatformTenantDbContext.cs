using Microsoft.EntityFrameworkCore;
using Platform.Host.Entities;
using Platform.ORM.EntityFramework.DataStorages;
using Platform.OrgStructure.EntityFramework.Extensions;

namespace Platform.Host.Postgresql.Persistence;

/// <summary>
/// DbContext tenant mặc định: map bộ entity org-structure chuẩn.
/// Product có bảng riêng thì kế thừa class này. Product đổi shape entity thì không kế thừa.
/// </summary>
public class PlatformTenantDbContext : BaseStorageContext
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
        modelBuilder.Entity<OrgUnit>().ToTable("OrgStructureUnit");
        modelBuilder.Entity<LegalEntity>(entity =>
        {
            entity.ToTable("OrgStructureLegalEntity");
            entity.Property(item => item.DetailsJson);
        });
        modelBuilder.Entity<JobTitle>().ToTable("OrgStructureJobTitle");
        modelBuilder.Entity<OrgChartPosition>().ToTable("OrgStructurePosition");
        modelBuilder.Entity<OrgUnitType>().ToTable("OrgStructureUnitType");
        modelBuilder.Entity<JobGrade>().ToTable("OrgStructureJobGrade");
        modelBuilder.Entity<JobPosition>().ToTable("OrgStructureJobPosition");
        modelBuilder.Entity<OrgUnitMember>().ToTable("OrgStructureUnitMember");
    }
}
