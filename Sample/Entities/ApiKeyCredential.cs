using Platform.DDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.Authentication;

namespace Sample.Entities;

/// <summary>API key theo tenant + profile — chỉ lưu hash.</summary>
public class ApiKeyCredential : BaseEntity<Guid>
{
    public Guid TenantId { get; set; }

    public required string Profile { get; set; }

    public required string KeyHash { get; set; }

    public string? OwnerName { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ApiKeyCredentialEntityConfiguration : IEntityTypeConfiguration<ApiKeyCredential>
{
    public static readonly Guid DefaultDevKeyId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid IntegrationDevKeyId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public const string DefaultDevKey = "dev-sample-api-key";
    public const string IntegrationDevKey = "dev-integration-api-key";

    public void Configure(EntityTypeBuilder<ApiKeyCredential> builder)
    {
        builder.ToTable("ApiKeyCredential");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Profile).IsRequired().HasMaxLength(64);
        builder.Property(x => x.KeyHash).IsRequired().HasMaxLength(512);
        builder.HasIndex(x => new { x.TenantId, x.Profile, x.KeyHash }).IsUnique();
        builder.Property(x => x.OwnerName).HasMaxLength(256);
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasData(
            new ApiKeyCredential
            {
                Id = DefaultDevKeyId,
                TenantId = SampleAuthTenants.DevTenantId,
                Profile = "Default",
                KeyHash = AuthenticationSecretHashing.HashApiKey(DefaultDevKey),
                OwnerName = "sample-default",
                IsActive = true
            },
            new ApiKeyCredential
            {
                Id = IntegrationDevKeyId,
                TenantId = SampleAuthTenants.DevTenantId,
                Profile = "Integration",
                KeyHash = AuthenticationSecretHashing.HashApiKey(IntegrationDevKey),
                OwnerName = "sample-integration",
                IsActive = true
            });
    }
}
