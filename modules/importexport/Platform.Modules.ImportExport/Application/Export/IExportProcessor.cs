using Platform.Modules.ImportExport.Application.Export.Models;

namespace Platform.Modules.ImportExport.Application.Export;

/// <summary>
/// Low-level export pipeline (MiniExcel). Prefer <see cref="IExportService"/> from consumers.
/// </summary>
public interface IExportProcessor
{
    /// <summary>Chạy export trên <see cref="ExportWork{T}"/> đã resolve template.</summary>
    Task<ExportResult> ExportAsync<T>(ExportWork<T> work, CancellationToken cancellationToken)
        where T : class;
}
