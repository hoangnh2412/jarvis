using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.ImportExport.Domain.Exceptions;

namespace Sample.ImportExport.Middleware;

/// <summary>
/// Maps ImportExport engine exceptions to Platform <see cref="BusinessException"/>
/// so <c>ApiResponseWrapperMiddleware</c> can format <c>BaseResponse</c>.
/// </summary>
public sealed class ImportExportExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ExcelEngineException ex) when (IsImportExportPath(context))
        {
            throw Map(ex);
        }
    }

    private static bool IsImportExportPath(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        return path.StartsWith("/api/v1/templates", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/api/v1/importexport", StringComparison.OrdinalIgnoreCase);
    }

    private static BusinessException Map(ExcelEngineException exception) => exception switch
    {
        TemplateNotFoundException => new NotFoundException(BaseErrorCode.Error, exception.Message, exception),
        ImportExportEntityNotFoundException => new NotFoundException(BaseErrorCode.Error, exception.Message, exception),
        TemplateAlreadyExistsException => new ConflictException(BaseErrorCode.Error, exception.Message, exception),
        InvalidTemplateException or UnsupportedFileException or FileTooLargeException
            or RowLimitExceededException or ExcelParsingException
            => new BadRequestException(BaseErrorCode.Error, exception.Message, exception),
        OperationTimeoutException => new BadRequestException(BaseErrorCode.Error, exception.Message, exception),
        _ => new BadRequestException(BaseErrorCode.Error, exception.Message, exception)
    };
}
