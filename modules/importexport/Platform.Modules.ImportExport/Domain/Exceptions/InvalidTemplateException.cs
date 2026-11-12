using System.Net;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class InvalidTemplateException : BusinessException
{
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.BadRequest;

    public InvalidTemplateException(string message)
        : base(BaseErrorCode.Error, message)
    {
    }
}
