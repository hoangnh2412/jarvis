using Jarvis.OpenTelemetry.SemanticConvention;
using Microsoft.Extensions.Logging;

namespace Jarvis.OpenTelemetry;

public static class LoggerExtension
{
    public static void LogException(this ILogger logger, Exception exception, string? message = null, params object?[] args)
    {
        var template = $"{ExceptionAttributes.Source}.{ExceptionAttributes.Type}: [{ExceptionAttributes.Code}] - {ExceptionAttributes.SystemMessage}\n{ExceptionAttributes.StackTrace}";
        if (!string.IsNullOrEmpty(message))
            template = "{Message} - " + template;

        if (exception.InnerException is not null)
            template += $"\nInnerException: {ExceptionAttributes.InnerSource}.{ExceptionAttributes.InnerType}: [{ExceptionAttributes.InnerCode}] {ExceptionAttributes.InnerMessage}\n{ExceptionAttributes.InnerStackTrace}";

        if (exception.InnerException is null)
            logger.LogError(exception, template, message, exception.Source, exception.GetType().Name, exception.HResult, exception.Message, exception.StackTrace, args);
        else
            logger.LogError(exception, template, message, exception.Source, exception.GetType().Name, exception.HResult, exception.Message, exception.StackTrace, exception.InnerException.Source, exception.InnerException.GetType().Name, exception.InnerException.HResult, exception.InnerException.Message, exception.InnerException.StackTrace, args);
    }
}
