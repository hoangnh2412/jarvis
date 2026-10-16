using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class UnsupportedFileException : ExcelEngineException
    {
        public UnsupportedFileException(string fileName)
            : base(ImportExportErrorCode.UnsupportedFile, EngineMessages.UnsupportedFile(fileName))
        {
        }
    }
