using Jarvis.WebApi.Monitoring.Interfaces;
using OpenTelemetry.Metrics;

namespace Jarvis.WebApi.Monitoring.Uptrace;

public class OTLPMetricExporter : IMetricExporter
{
    private readonly OTLPOption _options;

    public OTLPMetricExporter(
        OTLPOption options)
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