using Platform.Modules.ImportExport.Application.Import;
using Platform.Modules.ImportExport.Application.Import.Models;
using Platform.Modules.ImportExport.Domain.Models;
using Sample.ImportExport.Constants;

namespace Sample.ImportExport.Services;

/// <summary>
/// Stub persister for User import — register keyed DI; implement SaveAsync when User import is added.
/// </summary>
public sealed class UserImportModulePersister : IImportModulePersister
{
    public string EntityKey => ImportExportEntityKeys.User;

    public Task<ImportPersistResult> SaveAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        TemplateDefinition template,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new ImportPersistResult
        {
            SavedCount = 0,
            Errors =
            [
                new ImportRowError(
                    RowNumber: 0,
                    Message: "User import persist is not implemented yet.",
                    ErrorCode: ImportErrorCodes.PersistFailed,
                    SheetName: template.SheetName)
            ]
        });
}
