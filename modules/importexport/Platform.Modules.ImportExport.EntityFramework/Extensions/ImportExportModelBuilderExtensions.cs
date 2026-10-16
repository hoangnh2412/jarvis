using Microsoft.EntityFrameworkCore;
using Platform.Modules.ImportExport.EntityFramework.Configuration;
using Platform.Modules.ImportExport.EntityFramework.EntityConfigurations;

namespace Platform.Modules.ImportExport.EntityFramework.Extensions;

/// <summary>
/// Áp dụng mapping entity Excel template mặc định lên <see cref="ModelBuilder"/> của host.
/// </summary>
public static class ImportExportModelBuilderExtensions
{
    /// <summary>
    /// Cấu hình entity Excel template đi kèm package.
    /// Ghi đè bảng/schema qua <paramref name="optionsAction"/> khi host cần layout riêng.
    /// </summary>
    public static ModelBuilder ConfigureExcelTemplate(
        this ModelBuilder modelBuilder,
        Action<ExcelTemplateModelBuilderConfigurationOptions>? optionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var options = new ExcelTemplateModelBuilderConfigurationOptions();
        optionsAction?.Invoke(options);

        modelBuilder.ApplyConfiguration(new ExcelTemplateEntityConfiguration(options));
        return modelBuilder;
    }
}
