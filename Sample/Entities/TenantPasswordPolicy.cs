using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Authentication;
using Platform.DDD.Domain.Entities;
using Sample.Authentication;

namespace Sample.Entities;

public class TenantPasswordPolicy : BaseEntity<Guid>
{
    public Guid TenantId { get; set; }

    public int MinLength { get; set; } = 8;

    public bool RequireDigit { get; set; }

    public bool RequireUppercase { get; set; }

    public bool RequireLowercase { get; set; }

    public bool RequireNonAlphanumeric { get; set; }

    public int MaxFailedAttempts { get; set; }

    public PasswordPolicyOptions ToOptions() => new()
    {
        MinLength = MinLength,
        RequireDigit = RequireDigit,
        RequireUppercase = RequireUppercase,
        RequireLowercase = RequireLowercase,
        RequireNonAlphanumeric = RequireNonAlphanumeric,
        MaxFailedAttempts = MaxFailedAttempts,
    };
}

public class TenantPasswordPolicyEntityConfiguration : IEntityTypeConfiguration<TenantPasswordPolicy>
{
    public static readonly Guid DevPolicyId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public void Configure(EntityTypeBuilder<TenantPasswordPolicy> builder)
    {
        builder.ToTable("TenantPasswordPolicy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.HasIndex(x => x.TenantId).IsUnique();

        builder.HasData(new TenantPasswordPolicy
        {
            Id = DevPolicyId,
            TenantId = SampleAuthTenants.DevTenantId,
            MinLength = 8,
            RequireDigit = true,
            RequireLowercase = true,
            RequireUppercase = false,
            RequireNonAlphanumeric = false,
            MaxFailedAttempts = 5,
        });
    }
}
