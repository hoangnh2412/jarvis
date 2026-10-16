using Microsoft.Extensions.Logging;
using Platform.Modules.ImportExport.Application.Import;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.EntityFramework.Stores;

/// <summary>
/// Default no-op <see cref="IImportDataPersister"/> (TryAdd trong <c>AddLlaExcelEngine</c>).
/// Production: đăng ký persister thật <b>trước</b> <c>AddLlaExcelEngine</c> để ghi đè.
/// </summary>
public sealed class DefaultImportDataPersister(ILogger<DefaultImportDataPersister> logger) : IImportDataPersister
{
    public Task<ImportPersistResult> SaveAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        TemplateDefinition template,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug(
            EngineLogMessages.PersistNoOp,
            template.Code,
            rows.Count);
        return Task.FromResult(new ImportPersistResult { SavedCount = 0 });
    }
}
