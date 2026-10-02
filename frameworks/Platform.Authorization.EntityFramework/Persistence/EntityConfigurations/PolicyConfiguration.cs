namespace Platform.Authorization.EntityFramework.Persistence.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Authorization.Domain;

/// <summary>
/// Cấu hình EF cho thực thể Policy.
/// Ánh xạ tới schema PostgreSQL "lexora" bảng "policies".
/// </summary>
public sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("policies", "lexora");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever(); // Guid được gán trong mã

        // Code là định danh duy nhất cho tra cứu thời gian chạy
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.ResourceType)
            .HasMaxLength(100);

        builder.Property(p => p.Action)
            .HasMaxLength(100);

        builder.Property(p => p.IsSystemPolicy)
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // One-to-many: Policy → PolicyRules
        builder.HasMany(p => p.Rules)
            .WithOne(r => r.Policy)
            .HasForeignKey(r => r.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
