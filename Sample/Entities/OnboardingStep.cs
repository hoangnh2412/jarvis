using Platform.DDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sample.Entities;

public class OnboardingStep : BaseEntity<Guid>
{
    public Guid? UserId { get; set; }
    public required string StepCode { get; set; }
    public required string Title { get; set; }
    public required string Department { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed
    public string? AssignedTo { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public AppUser? User { get; set; }
    public BusinessWorkflowMapping? WorkflowMapping { get; set; }
}

public class OnboardingStepConfiguration : IEntityTypeConfiguration<OnboardingStep>
{
    public void Configure(EntityTypeBuilder<OnboardingStep> builder)
    {
        builder.ToTable("OnboardingSteps", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.UserId);
        builder.Property(x => x.StepCode).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.StepCode }).IsUnique();
        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Department).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Order).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.AssignedTo).HasMaxLength(128);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        builder.HasOne(x => x.User)
            .WithMany(x => x.OnboardingSteps)
            .HasForeignKey(x => x.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
