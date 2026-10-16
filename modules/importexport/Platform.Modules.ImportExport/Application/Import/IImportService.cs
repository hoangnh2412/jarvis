using Platform.Modules.ImportExport.Application.Import.Models;

namespace Platform.Modules.ImportExport.Application.Import;

/// <summary>
/// Public import façade (API host và project khác gọi DLL đều dùng interface này).
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Import Excel/CSV theo <see cref="ImportRequest.TemplateCode"/>.
    /// Engine load layout từ <see cref="ITemplateService"/> rồi chạy processor.
    /// </summary>
    Task<ImportResult> ImportAsync(
        ImportRequest request,
        CancellationToken cancellationToken = default);
}
