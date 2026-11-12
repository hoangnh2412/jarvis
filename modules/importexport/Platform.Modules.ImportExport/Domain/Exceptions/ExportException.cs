using System.Net;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class ExportException : BusinessException
{
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.BadRequest;

    public ExportException(string message)
        : base(BaseErrorCode.Error, message)
    {
    }

    public ExportException(string message, Exception innerException)
        : base(BaseErrorCode.Error, message, innerException)
    {
    }
}
