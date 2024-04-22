using OpenTelemetry.Metrics;

namespace Jarvis.WebApi.Monitoring.Interfaces;

public interface IMetricInstrumentation
{
    MeterProviderBuilder AddInstrumentation(MeterProviderBuilder builder);
}