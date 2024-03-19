using OpenTelemetry.Logs;

namespace Jarvis.WebApi.Monitoring;

public interface ILoggingExporter
{
    OpenTelemetryLoggerOptions AddExporter(OpenTelemetryLoggerOptions builder);
}