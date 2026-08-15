using Jarvis.OpenTelemetry.Abstractions;

namespace Sample.Telemetry;

/// <summary>
/// Case 3 — attribute chung cho cả log và trace (qua default EnrichLog/EnrichTrace).
/// </summary>
public sealed class SampleSharedEnrichmentSource : IEnrichmentSource
{
    public Task<Dictionary<string, string>> ExtractAsync() =>
        Task.FromResult(new Dictionary<string, string>
        {
            ["sample.enrich.shared"] = "true",
        });
}
