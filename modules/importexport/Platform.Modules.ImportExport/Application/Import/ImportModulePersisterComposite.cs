using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Platform.Modules.ImportExport.Application.Import.Models;
using Platform.Modules.ImportExport.Domain.Constants;
using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Import;

/// <summary>
/// Routes persist to keyed <see cref="IImportModulePersister"/> by <see cref="TemplateDefinition.EntityKey"/>.
/// </summary>
public sealed class ImportModulePersisterComposite(
    IServiceProvider serviceProvider,
    ILogger<ImportModulePersisterComposite> logger) : IImportDataPersister
{
    public Task<ImportPersistResult> SaveAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        TemplateDefinition template,
        CancellationToken cancellationToken = default)
    {
        var key = template.EntityKey?.Trim();
        if (string.IsNullOrEmpty(key))
        {
            const string message =
                "Import EntityKey is required. Set ImportRequest.EntityKey from the host module service.";
            logger.LogWarning(
                EngineLogMessages.PersistRoutingFailed,
                template.Code,
                key,
                message);
            return Task.FromResult(PersistError(row: 0, message));
        }

        var handlers = serviceProvider.GetServices<IImportModulePersister>();
        var handler = handlers.FirstOrDefault(h =>
            string.Equals(h.EntityKey, key, StringComparison.OrdinalIgnoreCase));

        if (handler is null)
        {
            var message = $"No IImportModulePersister registered for entity '{key}'.";
            logger.LogWarning(
                EngineLogMessages.PersistRoutingFailed,
                template.Code,
                key,
                message);
            return Task.FromResult(PersistError(row: 0, message));
        }

        return handler.SaveAsync(rows, template, cancellationToken);
    }

    private static ImportPersistResult PersistError(int row, string message) =>
        new()
        {
            SavedCount = 0,
            Errors =
            [
                new ImportRowError(row, message, ErrorCode: ImportErrorCodes.PersistFailed)
            ]
        };
}
