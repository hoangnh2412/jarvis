using OpenTelemetry.Exporter;

namespace Jarvis.OpenTelemetry.Configuration;

/// <summary>
/// Trace pipeline settings: OTLP (see <see cref="OtlpExporterOptions"/>), sampling, ASP.NET / HttpClient trace instrumentation, and HTTP header span enrichment.
/// </summary>
public class TraceSignalOptions : OtlpExporterOptions
{
    /// <summary>
    /// <c>AlwaysOn</c>, <c>AlwaysOff</c>, <c>ParentBasedRatio</c> (default), or <c>TraceIdRatio</c>.
    /// </summary>
    public string Sampler { get; set; } = "ParentBasedRatio";

    /// <summary>Sampling probability (0.0–1.0). Used by <c>ParentBasedRatio</c> for root and remote unsampled branches.</summary>
    public double TraceIdRatio { get; set; } = 1.0;

    public bool IncludeConsoleExporter { get; set; }

    public HttpTraceEnrichmentOptions HttpTraceEnrichment { get; set; } = new();

    public AspNetCoreTraceInstrumentationOptions AspNetCoreInstrumentation { get; set; } = new();

    public HttpClientTraceInstrumentationOptions HttpClient { get; set; } = new();
}
