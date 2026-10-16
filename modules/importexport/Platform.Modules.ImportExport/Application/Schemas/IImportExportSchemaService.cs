using Platform.Modules.ImportExport.Application.Schemas.Models;

namespace Platform.Modules.ImportExport.Application.Schemas;

public interface IImportExportSchemaService
{
    IReadOnlyList<ImportExportEntitySummary> ListEntities();

    IReadOnlyList<ImportExportFieldSchema> GetFields(string key);
}
