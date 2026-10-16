using Platform.Modules.ImportExport.Application.Import.Models;

namespace Platform.Modules.ImportExport.Application.Import;

/// <summary>
/// Low-level import pipeline (MiniExcel). Prefer <see cref="IImportService"/> from consumers.
/// </summary>
public interface IImportProcessor
{
    /// <summary>Chạy import trên <see cref="ImportWork"/> đã resolve template.</summary>
    Task<ImportResult> ImportAsync(ImportWork work, CancellationToken cancellationToken);
}
