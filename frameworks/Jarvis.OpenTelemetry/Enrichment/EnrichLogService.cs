using Jarvis.OpenTelemetry.Abstractions;

namespace Jarvis.OpenTelemetry.Enrichment;

/// <summary>
/// Default log enricher that merges all registered <see cref="IEnrichmentSource"/> instances.
/// </summary>
public sealed class EnrichLogService(IEnumerable<IEnrichmentSource> sources) : IEnrichLogService
{
    public Task<Dictionary<string, string>> ExtractAsync() =>
        EnrichmentSourceMerger.MergeAsync(sources);
}
