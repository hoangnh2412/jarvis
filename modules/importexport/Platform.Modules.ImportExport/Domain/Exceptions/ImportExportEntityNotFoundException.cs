using System.Net;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class ImportExportEntityNotFoundException : BusinessException
{
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.NotFound;

    public ImportExportEntityNotFoundException(string key)
        : base(BaseErrorCode.Error, EngineMessages.ImportExportEntityNotFound(key))
    {
    }
}
