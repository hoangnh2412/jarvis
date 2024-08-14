using OpenTelemetry.Metrics;

namespace Jarvis.OpenTelemetry.Interfaces;

public interface IMetricExporter
{
    MeterProviderBuilder AddExporter(MeterProviderBuilder builder);
}