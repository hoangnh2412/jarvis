using Platform.DDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.Authentication;

namespace Sample.Entities;

public class BasicAuthUser : BaseEntity<Guid>
{
    public Guid TenantId { get; set; }

    public required string Username { get; set; }

    public required string PasswordHash { get; set; }

    public string[] Roles { get; set; } = [];
}

public class BasicAuthUserEntityConfiguration : IEntityTypeConfiguration<BasicAuthUser>
{
    public static readonly Guid SampleUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public void Configure(EntityTypeBuilder<BasicAuthUser> builder)
    {
        builder.ToTable("BasicAuthUser");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Username).IsRequired().HasMaxLength(128);
        builder.HasIndex(x => new { x.TenantId, x.Username }).IsUnique();
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(x => x.Roles);

        builder.HasData(new BasicAuthUser
        {
            Id = SampleUserId,
            TenantId = SampleAuthTenants.DevTenantId,
            Username = "sampleuser",
            PasswordHash = AuthenticationSecretHashing.HashBasicPassword("samplepass"),
            Roles = ["user", "admin"]
        });
    }
}
