using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Modules.ImportExport.EntityFramework.Configuration;
using ExcelTemplateEntity = Platform.Modules.ImportExport.EntityFramework.Entities.ExcelTemplate;

namespace Platform.Modules.ImportExport.EntityFramework.EntityConfigurations;

/// <summary>
/// Mapping EF Core mặc định cho <see cref="ExcelTemplateEntity"/>
/// (unique <c>TenantId + Code</c>, soft-delete qua <c>IsActive</c>).
/// </summary>
public sealed class ExcelTemplateEntityConfiguration : IEntityTypeConfiguration<ExcelTemplateEntity>
{
    private readonly ExcelTemplateModelBuilderConfigurationOptions _options;

    public ExcelTemplateEntityConfiguration()
        : this(new ExcelTemplateModelBuilderConfigurationOptions())
    {
    }

    public ExcelTemplateEntityConfiguration(ExcelTemplateModelBuilderConfigurationOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public void Configure(EntityTypeBuilder<ExcelTemplateEntity> builder)
    {
        if (string.IsNullOrWhiteSpace(_options.Schema))
            builder.ToTable(_options.TableName);
        else
            builder.ToTable(_options.TableName, _options.Schema);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.Location).HasMaxLength(1000);
        builder.Property(x => x.MappingsJson).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasQueryFilter(x => x.IsActive);
    }
}
