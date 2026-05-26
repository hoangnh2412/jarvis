namespace Jarvis.OpenTelemetry.SemanticConventions;

/// <summary>
/// Cross-cutting trace and correlation fields used in log scopes and span tags.
/// </summary>
public static class TelemetryContextAttributes
{
    public const string TraceId = "trace.id";
    public const string SpanId = "span.id";
    public const string CorrelationId = "correlation.id";
    public const string TraceParent = "traceparent";
    public const string OperationName = "operation.name";
    public const string WorkerName = "worker.name";
}
