using System.Net;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class TemplateNotFoundException : BusinessException
{
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.NotFound;

    public TemplateNotFoundException(string code)
        : base(BaseErrorCode.Error, EngineMessages.TemplateNotFound(code))
    {
    }
}

public sealed class TemplateAlreadyExistsException : BusinessException
{
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.Conflict;

    public TemplateAlreadyExistsException(string code)
        : base(BaseErrorCode.Error, EngineMessages.TemplateAlreadyExists(code))
    {
    }
}
