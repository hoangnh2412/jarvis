using OpenTelemetry.Metrics;

namespace Platform.OpenTelemetry.Abstractions;

/// <summary>
/// Additional metric exporters. Register as singleton.
/// </summary>
public interface IMetricExporter
{
    MeterProviderBuilder AddExporter(MeterProviderBuilder builder);
}
