using OpenTelemetry.Trace;

namespace Jarvis.OpenTelemetry.Interfaces;

public interface ITraceInstrumentation
{
    TracerProviderBuilder AddInstrumentation(TracerProviderBuilder builder);
}