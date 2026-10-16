using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class FileTooLargeException : ExcelEngineException
    {
        public FileTooLargeException(long actualSize, long maxSize)
            : base(ImportExportErrorCode.FileTooLarge, EngineMessages.FileTooLarge(actualSize, maxSize))
        {
        }
    }
