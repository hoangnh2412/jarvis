namespace Jarvis.OpenTelemetry.Abstractions;

public interface IEnrichTraceService
{
    Task<Dictionary<string, string>> ExtractAsync();
}
