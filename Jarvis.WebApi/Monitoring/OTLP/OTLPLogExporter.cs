using Jarvis.WebApi.Monitoring.Interfaces;
using OpenTelemetry.Logs;

namespace Jarvis.WebApi.Monitoring.Uptrace;

public class OTLPLogExporter : ILoggingExporter
{
    private readonly OTLPOption _options;

    public OTLPLogExporter(
        OTLPOption options)
    {
        _options = options;
    }

    public OpenTelemetryLoggerOptions AddExporter(OpenTelemetryLoggerOptions builder)
    {
        builder.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(_options.Logging.Endpoint);
        });
        return builder;
    }
}