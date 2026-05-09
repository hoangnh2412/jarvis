namespace Jarvis.OpenTelemetry.Instrumentations;

internal static class HttpHeaderEnrichmentHelper
{
    public static string NormalizeHeaderNameForAttribute(string headerName) =>
        headerName.ToLowerInvariant();

    public static HashSet<string> ToAllowedSet(IList<string> names)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in names)
        {
            if (!string.IsNullOrWhiteSpace(n))
                set.Add(n.Trim());
        }

        return set;
    }

    public static string Truncate(string value, int maxLength)
    {
        if (maxLength <= 0 || value.Length <= maxLength)
            return value;
        return value[..maxLength];
    }
}
