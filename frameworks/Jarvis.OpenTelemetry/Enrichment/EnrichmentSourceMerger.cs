using Jarvis.OpenTelemetry.Abstractions;

namespace Jarvis.OpenTelemetry.Enrichment;

internal static class EnrichmentSourceMerger
{
    public static async Task<Dictionary<string, string>> MergeAsync(
        IEnumerable<IEnrichmentSource> sources)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var source in sources)
        {
            var data = await source.ExtractAsync().ConfigureAwait(false);
            foreach (var (key, value) in data)
                result[key] = value;
        }

        return result;
    }
}
