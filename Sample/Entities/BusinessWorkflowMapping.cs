using Platform.DDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sample.Entities;

public class BusinessWorkflowMapping : BaseEntity<Guid>
{
    public Guid? UserId { get; set; }
    public Guid? StepId { get; set; }
    public string? StepCode { get; set; }
    public required string WorkflowDefinitionId { get; set; }
    /// <summary>
    /// ID của workflow instance cha (ParentWorkflowInstanceId trong bảng elsa.WorkflowInstances)
    /// </summary>
    public string? InstanceId { get; set; }
    public required string WorkflowName { get; set; }
    public string? Description { get; set; }
    public string WorkflowStatus { get; set; } = "NotStarted";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public AppUser? User { get; set; }
    public OnboardingStep? Step { get; set; }
}

public class BusinessWorkflowMappingConfiguration : IEntityTypeConfiguration<BusinessWorkflowMapping>
{
    public void Configure(EntityTypeBuilder<BusinessWorkflowMapping> builder)
    {
        builder.ToTable("BusinessWorkflowMappings", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.UserId);
        builder.Property(x => x.StepId);
        builder.Property(x => x.StepCode).HasMaxLength(64);
        builder.Property(x => x.WorkflowDefinitionId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.InstanceId).HasMaxLength(128);
        builder.Property(x => x.WorkflowName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.WorkflowStatus).HasMaxLength(32).IsRequired().HasDefaultValue("NotStarted");
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.WorkflowDefinitionId, x.InstanceId });
        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany(x => x.WorkflowMappings)
            .HasForeignKey(x => x.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Step)
            .WithOne(x => x.WorkflowMapping)
            .HasForeignKey<BusinessWorkflowMapping>(x => x.StepId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
