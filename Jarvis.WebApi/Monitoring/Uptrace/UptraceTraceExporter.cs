using Jarvis.WebApi.Monitoring.Interfaces;
using OpenTelemetry.Trace;

namespace Jarvis.WebApi.Monitoring.Uptrace;

public class UptraceTraceExporter : ITraceExporter
{
    private readonly OTLPOption _options;

    public UptraceTraceExporter(
        OTLPOption options)
    {
        _options = options;
    }

    public TracerProviderBuilder Confiture(TracerProviderBuilder builder)
    {
        builder.AddOtlpExporter(options =>
        {
            options.Endpoint = UptraceMonitoringExtension.ParseUptraceDsn(_options.Tracing.Endpoint);
            options.Headers = string.Format("uptrace-dsn={0}", _options.Tracing.Endpoint);
        });
        return builder;
    }
}