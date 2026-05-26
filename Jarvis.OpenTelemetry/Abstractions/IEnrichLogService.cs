namespace Jarvis.OpenTelemetry.Abstractions;

public interface IEnrichLogService
{
    Task<Dictionary<string, string>> ExtractAsync();
}
