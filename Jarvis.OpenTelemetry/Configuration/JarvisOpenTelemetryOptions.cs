namespace Jarvis.OpenTelemetry.Configuration;

/// <summary>
/// Root options bound from configuration section <c>OTEL</c>.
/// Values are snapshotted when <c>AddJarvisOpenTelemetry</c> runs (startup); use environment variables (e.g. <c>OTEL_*</c>) for exporter defaults where supported.
/// </summary>
public class JarvisOpenTelemetryOptions
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
    public Dictionary<string, string> Attributes { get; set; } = new(StringComparer.Ordinal);
    public OpenTelemetryLoggingOptions Logging { get; set; } = new();
    public MetricTelemetryOptions Metric { get; set; } = new();
    public TraceSignalOptions Tracing { get; set; } = new();
    public ResourceTelemetryOptions Resource { get; set; } = new();
}
