using OpenTelemetry.Trace;

namespace Jarvis.WebApi.Monitoring.Interfaces;

public interface ITraceInstrumentation
{
    TracerProviderBuilder Configure(IServiceProvider serviceProvider, TracerProviderBuilder builder);
}