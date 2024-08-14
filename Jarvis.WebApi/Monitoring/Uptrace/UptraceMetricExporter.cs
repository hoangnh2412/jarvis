using Jarvis.WebApi.Monitoring.Interfaces;
using OpenTelemetry.Metrics;

namespace Jarvis.WebApi.Monitoring.Uptrace;

public class UptraceMetricExporter : IMetricExporter
{
    private readonly OTLPOption _options;

    public UptraceMetricExporter(
        OTLPOption options)
    {
        _options = options;
    }

    public MeterProviderBuilder Confiture(MeterProviderBuilder builder)
    {
        builder.AddOtlpExporter(options =>
        {
            options.Endpoint = UptraceMonitoringExtension.ParseUptraceDsn(_options.Metric.Endpoint);
            options.Headers = string.Format("uptrace-dsn={0}", _options.Metric.Endpoint);
        });
        return builder;
    }
}