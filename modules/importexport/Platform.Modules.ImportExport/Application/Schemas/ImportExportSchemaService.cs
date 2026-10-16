using Microsoft.Extensions.Logging;
using Platform.Modules.ImportExport.Application.Schemas.Models;
using Platform.Modules.ImportExport.Domain.Constants;
using Platform.Modules.ImportExport.Domain.Exceptions;

namespace Platform.Modules.ImportExport.Application.Schemas;

public sealed class ImportExportSchemaService(
    IImportExportEntityRegistry registry,
    ILogger<ImportExportSchemaService> logger)
    : IImportExportSchemaService
{
    public IReadOnlyList<ImportExportEntitySummary> ListEntities()
    {
        logger.LogDebug(EngineLogMessages.SchemaListRequested);
        return registry.List();
    }

    public IReadOnlyList<ImportExportFieldSchema> GetFields(string key)
    {
        logger.LogDebug(EngineLogMessages.SchemaFieldsRequested, key);
        if (!registry.TryGet(key, out var entity))
            throw new ImportExportEntityNotFoundException(key);

        return ReflectionFieldReader.Read(entity.RowType);
    }
}
