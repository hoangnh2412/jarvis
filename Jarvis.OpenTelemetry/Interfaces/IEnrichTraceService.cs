namespace Jarvis.OpenTelemetry.Interfaces;

public interface IEnrichTraceService
{
    Task<Dictionary<string, string>> ExtractAsync();
}