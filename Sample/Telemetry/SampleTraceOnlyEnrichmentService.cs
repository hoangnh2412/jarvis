using Jarvis.OpenTelemetry.Abstractions;

namespace Sample.Telemetry;

/// <summary>
/// Case 2 — attribute chỉ vào trace tags (không vào log scope).
/// </summary>
public sealed class SampleTraceOnlyEnrichmentService : IEnrichTraceService
{
    public Task<Dictionary<string, string>> ExtractAsync() =>
        Task.FromResult(new Dictionary<string, string>
        {
            ["sample.enrich.trace_only"] = "true",
        });
}
