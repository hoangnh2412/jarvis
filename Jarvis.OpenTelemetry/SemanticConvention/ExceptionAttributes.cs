namespace Jarvis.OpenTelemetry.SemanticConvention;

/// <summary>
/// Semantic convention attributes in the exception. <br />
/// https://opentelemetry.io/docs/specs/semconv/exceptions/exceptions-logs/#recording-an-exception
/// </summary>
public class ExceptionAttributes
{
    public const string Source = "{exception.source}";

    /// <summary>
    /// The type of the exception. Ex: ArgumentException, ArgumentNullException, BusinessException, ...
    /// </summary>
    public const string Type = "{exception.type}";

    public const string Code = "{exception.code}";

    /// <summary>
    /// The exception message
    /// </summary>
    public const string Message = "{exception.message}";

    /// <summary>
    /// A stacktrace as a string
    /// </summary>
    public const string StackTrace = "{exception.stacktrace}";

    /// <summary>
    /// The system message like exception message, http request exception message
    /// </summary>
    public const string SystemMessage = "{exception.system_message}";

    public const string InnerSource = "{exception.inner_source}";

    /// <summary>
    /// The type of the inner exception. Ex: ArgumentException, ArgumentNullException, BusinessException, ...
    /// </summary>
    public const string InnerType = "{exception.inner_type}";

    public const string InnerCode = "{exception.inner_code}";

    /// <summary>
    /// The inner exception message
    /// </summary>
    public const string InnerMessage = "{exception.inner_message}";

    /// <summary>
    /// A stacktrace as a string of the inner exception
    /// </summary>
    public const string InnerStackTrace = "{exception.inner_stacktrace}";
}