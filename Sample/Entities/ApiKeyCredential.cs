using Jarvis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sample.Entities;

/// <summary>API key lưu trong Master DB — dùng khi <c>Authentication:CredentialSource=Database</c>.</summary>
public class ApiKeyCredential : BaseEntity<Guid>
{
    public required string Key { get; set; }

    public required string OwnerName { get; set; }

    public string[] Roles { get; set; } = [];
}

public class ApiKeyCredentialEntityConfiguration : IEntityTypeConfiguration<ApiKeyCredential>
{
    public static readonly Guid SampleKeyId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    /// <summary>Dev seed — plaintext demo only.</summary>
    public const string SampleKeyValue = "sample-db-api-key";

    public void Configure(EntityTypeBuilder<ApiKeyCredential> builder)
    {
        builder.ToTable("ApiKeyCredential");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.Key).IsRequired().HasMaxLength(256);
        builder.HasIndex(x => x.Key).IsUnique();
        builder.Property(x => x.OwnerName).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Roles);

        builder.HasData(new ApiKeyCredential
        {
            Id = SampleKeyId,
            Key = SampleKeyValue,
            OwnerName = "database",
            Roles = ["integration"]
        });
    }
}
