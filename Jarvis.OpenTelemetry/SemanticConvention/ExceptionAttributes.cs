namespace Jarvis.OpenTelemetry.SemanticConvention;

/// <summary>
/// Semantic convention attributes in the exception. <br />
/// https://opentelemetry.io/docs/specs/semconv/exceptions/exceptions-logs/#recording-an-exception
/// </summary>
public class ExceptionAttributes
{
    /// <summary>
    /// The system message like exception message, http request exception message
    /// </summary>
    public const string SystemMessage = "exception.system_message";
}