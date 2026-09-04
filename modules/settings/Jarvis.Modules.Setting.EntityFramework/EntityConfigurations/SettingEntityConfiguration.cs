using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Jarvis.Modules.Setting.EntityFramework.Configuration;
using SettingEntity = Jarvis.Modules.Setting.EntityFramework.Entities.Setting;

namespace Jarvis.Modules.Setting.EntityFramework.EntityConfigurations;

/// <summary>
/// Mapping EF Core mặc định cho <see cref="SettingEntity"/>
/// (unique <c>TenantId + Key</c>, index <c>TenantId + Group</c>).
/// </summary>
public sealed class SettingEntityConfiguration : IEntityTypeConfiguration<SettingEntity>
{
    private readonly SettingModelBuilderConfigurationOptions _options;

    /// <summary>Dùng options mặc định (bảng <c>Setting</c>, không schema).</summary>
    public SettingEntityConfiguration()
        : this(new SettingModelBuilderConfigurationOptions())
    {
    }

    /// <summary>Dùng options tuỳ chỉnh tên bảng / schema.</summary>
    public SettingEntityConfiguration(SettingModelBuilderConfigurationOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public void Configure(EntityTypeBuilder<SettingEntity> builder)
    {
        if (string.IsNullOrWhiteSpace(_options.Schema))
            builder.ToTable(_options.TableName);
        else
            builder.ToTable(_options.TableName, _options.Schema);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Group).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Key).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Value).IsRequired();
        builder.Property(x => x.Type).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Options).HasMaxLength(1024);
        builder.Property(x => x.Description).HasMaxLength(1024);
        builder.Property(x => x.IsReadOnly).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Group });
    }
}
