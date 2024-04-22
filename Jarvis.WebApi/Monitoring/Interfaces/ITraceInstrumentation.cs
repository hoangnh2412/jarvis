using OpenTelemetry.Trace;

namespace Jarvis.WebApi.Monitoring.Interfaces;

public interface ITraceInstrumentation
{
    TracerProviderBuilder AddInstrumentation(TracerProviderBuilder builder);
}