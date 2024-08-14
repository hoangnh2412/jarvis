using OpenTelemetry.Metrics;
using Jarvis.OpenTelemetry.Interfaces;

namespace Jarvis.OpenTelemetry.Exporters;

public class OTELMetricExporter : IMetricExporter
{
    private readonly OTELOption _options;

    public OTELMetricExporter(
        OTELOption options)
    {
        _options = options;
    }

    public MeterProviderBuilder AddExporter(MeterProviderBuilder builder)
    {
        builder.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(_options.Metric.Endpoint);
        });
        return builder;
    }
}