namespace Jarvis.EntityFramework.Helpers;

internal static class PagedListRequestParser
{
    /// <summary>Comma-separated property names.</summary>
    public static IReadOnlyList<string> ParseColumns(string? columns)
    {
        if (string.IsNullOrWhiteSpace(columns))
            return Array.Empty<string>();

        return columns
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
