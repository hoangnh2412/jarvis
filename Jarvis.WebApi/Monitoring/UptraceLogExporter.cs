using OpenTelemetry.Logs;
using Uptrace.OpenTelemetry;

namespace Jarvis.WebApi.Monitoring;

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
        builder.AddUptrace(_options.Logging.Endpoint);
        return builder;
    }
}