using OpenTelemetry.Metrics;

namespace Jarvis.WebApi.Monitoring.Interfaces;

public interface IMetricExporter
{
    MeterProviderBuilder AddExporter(MeterProviderBuilder builder);
}