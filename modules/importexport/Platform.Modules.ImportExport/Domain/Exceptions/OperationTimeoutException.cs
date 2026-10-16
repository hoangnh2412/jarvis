using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

/// <summary>Engine soft timeout (ImportTimeoutSeconds / ExportTimeoutSeconds) elapsed.</summary>
public sealed class OperationTimeoutException : ExcelEngineException
{
    public string OperationName { get; }

    public int TimeoutSeconds { get; }

    public OperationTimeoutException(string operation, int timeoutSeconds)
        : base(
            ImportExportErrorCode.OperationTimeout,
            EngineMessages.OperationTimeout(operation, timeoutSeconds))
    {
        OperationName = operation;
        TimeoutSeconds = timeoutSeconds;
    }
}
