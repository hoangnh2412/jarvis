using Platform.Modules.ImportExport.Application.Export.Models;

namespace Platform.Modules.ImportExport.Application.Export;

/// <summary>
/// Public export façade (API host và project khác gọi DLL đều dùng interface này).
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Export data theo <see cref="ExportRequest{T}.TemplateCode"/>.
    /// Engine load mappings + template bytes từ <see cref="ITemplateService"/> rồi chạy processor.
    /// </summary>
    Task<ExportResult> ExportAsync<T>(
        ExportRequest<T> request,
        CancellationToken cancellationToken = default)
        where T : class;
}
