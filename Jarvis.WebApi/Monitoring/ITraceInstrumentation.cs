using OpenTelemetry.Trace;

namespace Jarvis.WebApi.Monitoring;

public interface ITraceInstrumentation
{
    TracerProviderBuilder AddInstrumentation(TracerProviderBuilder builder);
}