using Jarvis.OpenTelemetry.Abstractions;

namespace Sample.Telemetry;

/// <summary>
/// Case 1 — attribute chỉ vào log scope (không vào trace tags).
/// </summary>
public sealed class SampleLogOnlyEnrichmentService : IEnrichLogService
{
    public Task<Dictionary<string, string>> ExtractAsync() =>
        Task.FromResult(new Dictionary<string, string>
        {
            ["sample.enrich.log_only"] = "true",
        });
}
