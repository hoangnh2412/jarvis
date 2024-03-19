using OpenTelemetry.Metrics;

namespace Jarvis.WebApi.Monitoring;

public interface IMetricExporter
{
    MeterProviderBuilder AddExporter(MeterProviderBuilder builder);
}