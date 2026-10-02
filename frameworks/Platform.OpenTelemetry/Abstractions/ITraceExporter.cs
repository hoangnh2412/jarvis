using OpenTelemetry.Trace;

namespace Platform.OpenTelemetry.Abstractions;

/// <summary>
/// Additional trace exporters (beyond Platform defaults). Register as singleton; invoked when the tracer provider is built.
/// </summary>
public interface ITraceExporter
{
    TracerProviderBuilder AddExporter(TracerProviderBuilder builder);
}
