namespace Jarvis.OpenTelemetry.SemanticConventions;

/// <summary>
/// Placeholder names for structured log templates (not OTel log record fields).
/// See https://opentelemetry.io/docs/specs/semconv/exceptions/exceptions-logs/
/// </summary>
public static class ExceptionAttributes
{
    public const string Source = "{exception.source}";
    public const string Type = "{exception.type}";
    public const string Code = "{exception.code}";
    public const string Message = "{exception.message}";
    public const string StackTrace = "{exception.stacktrace}";
    public const string SystemMessage = "{exception.system_message}";
    public const string InnerSource = "{exception.inner_source}";
    public const string InnerType = "{exception.inner_type}";
    public const string InnerCode = "{exception.inner_code}";
    public const string InnerMessage = "{exception.inner_message}";
    public const string InnerStackTrace = "{exception.inner_stacktrace}";
}
