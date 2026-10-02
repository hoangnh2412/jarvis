namespace Platform.Authorization.EntityFramework.Persistence.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Authorization.Domain;

/// <summary>
/// Cấu hình EF cho thực thể PolicyRule.
/// Ánh xạ tới schema PostgreSQL "lexora" bảng "policy_rules".
/// </summary>
public sealed class PolicyRuleConfiguration : IEntityTypeConfiguration<PolicyRule>
{
    public void Configure(EntityTypeBuilder<PolicyRule> builder)
    {
        builder.ToTable("policy_rules", "lexora");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        // Khóa ngoại tới Policy
        builder.HasOne(r => r.Policy)
            .WithMany(p => p.Rules)
            .HasForeignKey(r => r.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Chỉ mục tổng hợp: duy nhất (PolicyId, Order)
        builder.HasIndex(r => new { r.PolicyId, r.Order }).IsUnique();

        builder.Property(r => r.LeftOperand)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.Operator)
            .IsRequired()
            .HasMaxLength(10); // "==", "!=", "<", "<=", ">", ">=", "IN"

        builder.Property(r => r.RightOperand)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.Order)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
