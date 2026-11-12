using System.Net;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class FileTooLargeException : BusinessException
{
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.BadRequest;

    public FileTooLargeException(long actualSize, long maxSize)
        : base(BaseErrorCode.Error, EngineMessages.FileTooLarge(actualSize, maxSize))
    {
    }
}
