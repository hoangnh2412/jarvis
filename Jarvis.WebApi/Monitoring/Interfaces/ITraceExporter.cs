using OpenTelemetry.Trace;

namespace Jarvis.WebApi.Monitoring.Interfaces;

public interface ITraceExporter
{
    TracerProviderBuilder AddExporter(TracerProviderBuilder builder);
}