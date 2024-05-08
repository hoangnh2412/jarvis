using OpenTelemetry.Metrics;

namespace Jarvis.WebApi.Monitoring.Interfaces;

public interface IMetricExporter
{
    MeterProviderBuilder Confiture(MeterProviderBuilder builder);
}