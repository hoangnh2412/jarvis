using OpenTelemetry.Metrics;

namespace Jarvis.OpenTelemetry.Interfaces;

public interface IMetricInstrumentation
{
    MeterProviderBuilder AddInstrumentation(MeterProviderBuilder builder);
}