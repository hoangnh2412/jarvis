using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class ImportExportEntityNotFoundException : ExcelEngineException
{
    public ImportExportEntityNotFoundException(string key)
        : base(ImportExportErrorCode.ImportExportEntityNotFound, EngineMessages.ImportExportEntityNotFound(key))
    {
    }
}
