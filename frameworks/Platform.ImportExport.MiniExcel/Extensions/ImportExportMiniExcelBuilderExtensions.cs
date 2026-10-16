using Platform.ImportExport.MiniExcel.DependencyInjection;
using Platform.Modules.ImportExport.DependencyInjection;

namespace Platform.ImportExport.MiniExcel.Extensions;

/// <summary>
/// Đăng ký MiniExcel processors cho module ImportExport.
/// </summary>
public static class ImportExportMiniExcelBuilderExtensions
{
    /// <summary>
    /// Đăng ký <see cref="Platform.Modules.ImportExport.Interfaces.IImportProcessor"/> /
    /// <see cref="Platform.Modules.ImportExport.Interfaces.IExportProcessor"/> mặc định (MiniExcel).
    /// </summary>
    public static PlatformImportExportBuilder UseMiniExcel(this PlatformImportExportBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HostBuilder.Services.AddPlatformImportExportMiniExcel();
        return builder;
    }
}
