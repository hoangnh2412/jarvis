using Jarvis.WebApi.Monitoring.Interfaces;
using OpenTelemetry.Logs;

namespace Jarvis.WebApi.Monitoring.Uptrace;

public class UptraceLogExporter : ILoggingExporter
{
    private readonly OTLPOption _options;

    public UptraceLogExporter(
        OTLPOption options)
    {
        _options = options;
    }

    public OpenTelemetryLoggerOptions AddExporter(OpenTelemetryLoggerOptions builder)
    {
        builder.AddOtlpExporter(options =>
        {
            options.Endpoint = UptraceMonitoringExtension.ParseUptraceDsn(_options.Logging.Endpoint);
            options.Headers = string.Format("uptrace-dsn={0}", _options.Logging.Endpoint);
        });
        return builder;
    }
}