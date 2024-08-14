using OpenTelemetry.Logs;
using Jarvis.OpenTelemetry.Interfaces;

namespace Jarvis.OpenTelemetry.Exporters;

public class OTELLogExporter : ILoggingExporter
{
    private readonly OTELOption _options;

    public OTELLogExporter(
        OTELOption options)
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