using OpenTelemetry.Trace;

namespace Jarvis.WebApi.Monitoring;

public interface ITraceExporter
{
    TracerProviderBuilder AddExporter(TracerProviderBuilder builder);
}