using OpenTelemetry.Metrics;

namespace Jarvis.WebApi.Monitoring.Interfaces;

public interface IMetricInstrumentation
{
    MeterProviderBuilder Configure(IServiceProvider serviceProvider, MeterProviderBuilder builder);
}