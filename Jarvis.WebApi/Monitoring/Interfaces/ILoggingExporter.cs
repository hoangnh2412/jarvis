using OpenTelemetry.Logs;

namespace Jarvis.WebApi.Monitoring.Interfaces;

public interface ILoggingExporter
{
    OpenTelemetryLoggerOptions AddExporter(OpenTelemetryLoggerOptions builder);
}