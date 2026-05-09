using OpenTelemetry.Exporter;

namespace Jarvis.OpenTelemetry.Configuration;

public class MetricTelemetryOptions : OtlpExporterOptions
{
    public bool IncludeConsoleExporter { get; set; }

    /// <summary>When <c>exponential</c>, uses base-2 exponential histogram buckets for applicable instruments.</summary>
    public string HistogramAggregation { get; set; } = string.Empty;
}
