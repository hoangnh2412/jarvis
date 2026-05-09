using OpenTelemetry.Metrics;

namespace Jarvis.OpenTelemetry.Abstractions;

public interface IMetricInstrumentation
{
    /// <param name="builder">Meter provider under construction.</param>
    /// <param name="serviceProvider">Root service provider when the meter provider is built; use for configuration-backed setup.</param>
    MeterProviderBuilder AddInstrumentation(IServiceProvider serviceProvider, MeterProviderBuilder builder);
}
