using OpenTelemetry.Logs;

namespace Jarvis.OpenTelemetry.Interfaces;

public interface ILoggingExporter
{
    OpenTelemetryLoggerOptions AddExporter(OpenTelemetryLoggerOptions builder);
}