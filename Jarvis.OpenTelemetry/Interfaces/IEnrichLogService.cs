namespace Jarvis.OpenTelemetry.Interfaces;

public interface IEnrichLogService
{
    Task<Dictionary<string, string>> ExtractAsync();
}