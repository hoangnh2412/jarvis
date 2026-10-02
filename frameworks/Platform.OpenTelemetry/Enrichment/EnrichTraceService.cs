using Platform.OpenTelemetry.Abstractions;

namespace Platform.OpenTelemetry.Enrichment;

/// <summary>
/// Default trace enricher that merges all registered <see cref="IEnrichmentSource"/> instances.
/// </summary>
public sealed class EnrichTraceService(IEnumerable<IEnrichmentSource> sources) : IEnrichTraceService
{
    public Task<Dictionary<string, string>> ExtractAsync() =>
        EnrichmentSourceMerger.MergeAsync(sources);
}
