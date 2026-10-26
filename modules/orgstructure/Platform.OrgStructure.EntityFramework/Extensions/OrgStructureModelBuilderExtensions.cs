using Microsoft.EntityFrameworkCore;
using Platform.OrgStructure;

namespace Platform.OrgStructure.EntityFramework.Extensions;

/// <summary>Map cột chuẩn. Bảng và cột riêng của host do host đặt tên.</summary>
public static class OrgStructureModelBuilderExtensions
{
    public static void ApplyPlatformOrgStructure<TUnit, TLegalEntity, TJobTitle, TPosition>(this ModelBuilder modelBuilder)
        where TUnit : class, IOrganizationalUnitEntity
        where TLegalEntity : class, ILegalEntityEntity
        where TJobTitle : class, IJobTitleEntity
        where TPosition : class, IPositionEntity
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<TUnit>(builder =>
        {
            builder.HasKey(unit => unit.Id);
            builder.Property(unit => unit.Code).HasMaxLength(OrgStructureLimits.CodeMaxLength).IsRequired();
            builder.Property(unit => unit.Name).HasMaxLength(OrgStructureLimits.NameMaxLength).IsRequired();
            builder.Property(unit => unit.UnitType).HasMaxLength(OrgStructureLimits.UnitTypeMaxLength).IsRequired();
            builder.Property(unit => unit.Path).HasMaxLength(OrgStructureLimits.PathMaxLength).IsRequired();
            builder.Property(unit => unit.IsActive).IsRequired();
            builder.HasIndex(unit => new { unit.OrgId, unit.Code }).IsUnique();
            builder.HasIndex(unit => unit.ParentId);
            builder.HasIndex(unit => unit.Path);
        });

        modelBuilder.Entity<TLegalEntity>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Code).HasMaxLength(OrgStructureLimits.CodeMaxLength).IsRequired();
            builder.Property(entity => entity.Name).HasMaxLength(OrgStructureLimits.NameMaxLength).IsRequired();
            builder.Property(entity => entity.TaxId).HasMaxLength(OrgStructureLimits.TaxIdMaxLength);
            builder.Property(entity => entity.IsActive).IsRequired();
            builder.HasIndex(entity => new { entity.OrgId, entity.Code }).IsUnique();
        });

        modelBuilder.Entity<TJobTitle>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Code).HasMaxLength(OrgStructureLimits.CodeMaxLength).IsRequired();
            builder.Property(entity => entity.Name).HasMaxLength(OrgStructureLimits.NameMaxLength).IsRequired();
            builder.HasIndex(entity => new { entity.OrgId, entity.Code }).IsUnique();
        });

        modelBuilder.Entity<TPosition>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.IsActive).IsRequired();
            builder.HasIndex(entity => entity.OrganizationalUnitId);
            builder.HasIndex(entity => entity.ReportsToPositionId);
        });
    }
}
