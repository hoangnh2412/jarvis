using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class ExcelParsingException : ExcelEngineException
{
    public ExcelParsingException(string message)
        : base(ImportExportErrorCode.ExcelParsingFailed, message)
    {
    }

    public ExcelParsingException(string message, Exception innerException)
        : base(ImportExportErrorCode.ExcelParsingFailed, message, innerException)
    {
    }
}
