using System.Text.RegularExpressions;

namespace Jarvis.Caching.Internal;

internal static partial class CacheKeyResolver
{
    private static readonly Regex PlaceholderRegex = PlaceholderPattern();

    internal static string Resolve(string template, IReadOnlyDictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(template);

        if (parameters.Count == 0)
        {
            EnsureNoUnresolvedPlaceholders(template);
            return template;
        }

        var key = PlaceholderRegex.Replace(template, match =>
        {
            var name = match.Groups[1].Value;
            return parameters.TryGetValue(name, out var value) ? value : match.Value;
        });

        EnsureNoUnresolvedPlaceholders(key);
        return key;
    }

    private static void EnsureNoUnresolvedPlaceholders(string key)
    {
        if (PlaceholderRegex.IsMatch(key))
        {
            throw new InvalidOperationException(
                $"Cache key '{key}' contains unresolved placeholders. Supply all parameters via CacheParam.WithParam.");
        }
    }

    [GeneratedRegex(@"\{([^{}]+)\}")]
    private static partial Regex PlaceholderPattern();
}
