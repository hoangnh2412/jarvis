using OpenTelemetry.Metrics;

namespace Jarvis.WebApi.Monitoring;

public interface IMetricInstrumentation
{
    MeterProviderBuilder AddInstrumentation(MeterProviderBuilder builder);
}