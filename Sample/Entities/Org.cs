using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.DDD.Domain.Entities;
using Platform.Orgs;

namespace Sample.Entities;

/// <summary>
/// Catalog Org của Sample. <see cref="TaxCode"/> là cột host — không có trên <c>IOrgManagementEntity</c>.
/// </summary>
public class Org : BaseEntity<Guid>, IOrgManagementEntity
{
    public Guid TenantId { get; set; }

    public Guid? ParentId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string? TaxCode { get; set; }
}

public class OrgEntityConfiguration : IEntityTypeConfiguration<Org>
{
    public void Configure(EntityTypeBuilder<Org> builder)
    {
        builder.ToTable("Org");
        builder.Property(x => x.TaxCode).HasMaxLength(32);
    }
}
