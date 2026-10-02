using OpenTelemetry.Logs;

namespace Platform.OpenTelemetry.Abstractions;

/// <summary>
/// Additional OpenTelemetry log pipeline configuration (e.g. extra exporters). Register as singleton.
/// </summary>
public interface ILoggingExporter
{
    void Configure(LoggerProviderBuilder builder, IServiceProvider serviceProvider);
}
