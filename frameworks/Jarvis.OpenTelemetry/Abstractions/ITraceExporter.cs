using OpenTelemetry.Trace;

namespace Jarvis.OpenTelemetry.Abstractions;

/// <summary>
/// Additional trace exporters (beyond Jarvis defaults). Register as singleton; invoked when the tracer provider is built.
/// </summary>
public interface ITraceExporter
{
    TracerProviderBuilder AddExporter(TracerProviderBuilder builder);
}
