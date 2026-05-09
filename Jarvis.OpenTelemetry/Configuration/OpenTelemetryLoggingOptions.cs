using OpenTelemetry.Exporter;

namespace Jarvis.OpenTelemetry.Configuration;

/// <summary>
/// OpenTelemetry logging provider + OTLP settings (see <see cref="OtlpExporterOptions"/>).
/// </summary>
public class OpenTelemetryLoggingOptions : OtlpExporterOptions
{
    public bool IncludeFormattedMessage { get; set; } = true;
    public bool IncludeScopes { get; set; } = true;
    public bool ParseStateValues { get; set; }

    public bool IncludeConsoleExporter { get; set; }
}
