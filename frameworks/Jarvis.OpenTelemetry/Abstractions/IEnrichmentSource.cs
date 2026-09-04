namespace Jarvis.OpenTelemetry.Abstractions;

/// <summary>
/// Provides key/value pairs that default <see cref="IEnrichLogService"/> /
/// <see cref="IEnrichTraceService"/> implementations merge into telemetry.
/// </summary>
public interface IEnrichmentSource
{
    Task<Dictionary<string, string>> ExtractAsync();
}
