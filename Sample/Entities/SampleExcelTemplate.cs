using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.DDD.Domain.Entities;
using Platform.Modules.ImportExport.Domain.Entities;

namespace Sample.Entities;

/// <summary>
/// Host-owned Excel template entity for Sample (implements <see cref="IExcelTemplateEntity"/>).
/// </summary>
public class SampleExcelTemplate : BaseEntity<Guid>, IExcelTemplateEntity
{
    public Guid TenantId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string MappingsJson { get; set; } = "[]";

    public int HeaderRow { get; set; } = 1;

    public int StartRow { get; set; } = 2;

    public int? RowCount { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class SampleExcelTemplateEntityConfiguration : IEntityTypeConfiguration<SampleExcelTemplate>
{
    public void Configure(EntityTypeBuilder<SampleExcelTemplate> builder)
    {
        builder.ToTable("excel_templates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.Location).HasMaxLength(1000);
        builder.Property(x => x.MappingsJson).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasQueryFilter(x => x.IsActive);
    }
}
