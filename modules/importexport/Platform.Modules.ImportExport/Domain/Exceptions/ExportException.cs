using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class ExportException : ExcelEngineException
    {
        public ExportException(string message)
            : base(
                ImportExportErrorCode.ExportFailed,
                message)
        {
        }

        public ExportException(
            string message,
            Exception innerException)
            : base(
                ImportExportErrorCode.ExportFailed,
                message,
                innerException)
        {
        }
    }
