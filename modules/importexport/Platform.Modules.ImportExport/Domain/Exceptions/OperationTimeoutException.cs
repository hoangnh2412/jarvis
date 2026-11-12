using System.Net;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

/// <summary>Engine soft timeout (ImportTimeoutSeconds / ExportTimeoutSeconds) elapsed.</summary>
public sealed class OperationTimeoutException : BusinessException
{
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.BadRequest;

    public string OperationName { get; }

    public int TimeoutSeconds { get; }

    public OperationTimeoutException(string operation, int timeoutSeconds)
        : base(BaseErrorCode.Error, EngineMessages.OperationTimeout(operation, timeoutSeconds))
    {
        OperationName = operation;
        TimeoutSeconds = timeoutSeconds;
    }
}
