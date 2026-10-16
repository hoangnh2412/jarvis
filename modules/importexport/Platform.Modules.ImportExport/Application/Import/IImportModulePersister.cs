using Platform.Modules.ImportExport.Application.Import.Models;
using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Import;

/// <summary>
/// Module-specific import persist handler — registered via keyed DI (<see cref="DependencyInjection.ImportModulePersisterExtensions"/>).
/// </summary>
public interface IImportModulePersister
{
    /// <summary>Must match <c>AddImportModulePersister</c> key and <c>ConfigureEntities</c> key.</summary>
    string EntityKey { get; }

    Task<ImportPersistResult> SaveAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        TemplateDefinition template,
        CancellationToken cancellationToken = default);
}
