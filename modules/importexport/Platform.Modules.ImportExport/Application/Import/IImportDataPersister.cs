using Platform.Modules.ImportExport.Domain.Models;
using Platform.Modules.ImportExport.Application.Import.Models;

namespace Platform.Modules.ImportExport.Application.Import;

/// <summary>
/// Nơi lưu data row sau import validate.
/// Production: implement và <c>AddScoped&lt;IImportDataPersister, MyPersister&gt;()</c>
/// <b>trước</b> <c>AddLlaExcelEngine()</c> (TryAdd default = no-op nếu bỏ qua).
/// Hỗ trợ mọi store: SQL, Cosmos, … tùy consumer.
/// </summary>
public interface IImportDataPersister
{
    /// <summary>
    /// Persist một batch row hợp lệ (có thể gọi nhiều lần theo <c>PersistBatchSize</c>).
    /// </summary>
    Task<ImportPersistResult> SaveAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        TemplateDefinition template,
        CancellationToken cancellationToken = default);
}
