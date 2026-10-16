using Platform.Modules.ImportExport.Application.Schemas.Models;

namespace Platform.Modules.ImportExport.Application.Schemas;

public interface IImportExportEntityRegistry
{
    IReadOnlyList<ImportExportEntitySummary> List();

    bool TryGet(string key, out RegisteredImportExportEntity entity);
}
