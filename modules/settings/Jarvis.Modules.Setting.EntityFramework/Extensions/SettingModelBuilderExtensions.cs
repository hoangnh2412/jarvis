using Microsoft.EntityFrameworkCore;
using Jarvis.Modules.Setting.EntityFramework.Configuration;
using Jarvis.Modules.Setting.EntityFramework.EntityConfigurations;

namespace Jarvis.Modules.Setting.EntityFramework.Extensions;

/// <summary>
/// Áp dụng mapping entity Setting mặc định lên <see cref="ModelBuilder"/> của host.
/// </summary>
public static class SettingModelBuilderExtensions
{
    /// <summary>
    /// Cấu hình entity Setting đi kèm package.
    /// Ghi đè bảng/schema qua <paramref name="optionsAction"/> khi host cần layout riêng;
    /// không truyền thì dùng mặc định (bảng <c>Setting</c>).
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder của DbContext host.</param>
    /// <param name="optionsAction">Tuỳ chỉnh tên bảng / schema (tuỳ chọn).</param>
    /// <returns>Chính <paramref name="modelBuilder"/> để gọi fluent tiếp.</returns>
    public static ModelBuilder ConfigureSetting(
        this ModelBuilder modelBuilder,
        Action<SettingModelBuilderConfigurationOptions>? optionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var options = new SettingModelBuilderConfigurationOptions();
        optionsAction?.Invoke(options);

        modelBuilder.ApplyConfiguration(new SettingEntityConfiguration(options));
        return modelBuilder;
    }
}
