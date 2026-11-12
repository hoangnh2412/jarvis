using System.Net;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class ExcelParsingException : BusinessException
{
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.BadRequest;

    public ExcelParsingException(string message)
        : base(BaseErrorCode.Error, message)
    {
    }

    public ExcelParsingException(string message, Exception innerException)
        : base(BaseErrorCode.Error, message, innerException)
    {
    }
}
