using System.Net;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class UnsupportedFileException : BusinessException
{
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.BadRequest;

    public UnsupportedFileException(string fileName)
        : base(BaseErrorCode.Error, EngineMessages.UnsupportedFile(fileName))
    {
    }
}
