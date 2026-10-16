using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class RowLimitExceededException : ExcelEngineException
    {
        public RowLimitExceededException(int actualRows, int maxRows)
            : base(ImportExportErrorCode.RowLimitExceeded, EngineMessages.RowLimitExceeded(actualRows, maxRows))
        {
        }
    }
