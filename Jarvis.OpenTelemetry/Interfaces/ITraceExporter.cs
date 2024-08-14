using OpenTelemetry.Trace;

namespace Jarvis.OpenTelemetry.Interfaces;

public interface ITraceExporter
{
    TracerProviderBuilder AddExporter(TracerProviderBuilder builder);
}