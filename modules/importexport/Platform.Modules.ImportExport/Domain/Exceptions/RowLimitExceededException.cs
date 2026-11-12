using System.Net;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class RowLimitExceededException : BusinessException
{
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.BadRequest;

    public RowLimitExceededException(int actualRows, int maxRows)
        : base(BaseErrorCode.Error, EngineMessages.RowLimitExceeded(actualRows, maxRows))
    {
    }
}
