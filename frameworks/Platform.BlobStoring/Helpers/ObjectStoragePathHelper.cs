namespace Platform.BlobStoring.Helpers;

/// <summary>
/// Shared prefix/path helpers for object storage rename and move.
/// </summary>
public static class ObjectStoragePathHelper
{
    public static string NormalizeKey(string path) => path.Replace('\\', '/').Trim('/');

    public static string CombineKey(string? basePath, string relativePath)
    {
        var left = string.IsNullOrEmpty(basePath) ? string.Empty : NormalizeKey(basePath);
        var right = NormalizeKey(relativePath);
        return string.IsNullOrEmpty(left) ? right : $"{left}/{right}";
    }

    public static string GetParentPrefix(string path)
    {
        var normalized = NormalizeKey(path);
        var slash = normalized.LastIndexOf('/');
        return slash < 0 ? string.Empty : normalized[..slash];
    }

    public static string GetLeafName(string path)
    {
        var normalized = NormalizeKey(path);
        var slash = normalized.LastIndexOf('/');
        return slash < 0 ? normalized : normalized[(slash + 1)..];
    }

    /// <summary>
    /// Builds the destination object path when moving into <paramref name="destinationFolder"/> without renaming.
    /// </summary>
    public static string BuildMoveDestinationPath(string sourcePath, string destinationFolder)
    {
        var leaf = GetLeafName(sourcePath);
        return CombineKey(destinationFolder, leaf);
    }

    /// <summary>
    /// Returns true when <paramref name="destinationFolder"/> is the source item or nested under it.
    /// </summary>
    public static bool IsDestinationInsideSource(string sourcePath, string destinationFolder)
    {
        var source = NormalizeKey(sourcePath);
        var destination = NormalizeKey(destinationFolder);

        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination))
            return false;

        return destination.Equals(source, StringComparison.Ordinal)
            || destination.StartsWith(source + "/", StringComparison.Ordinal);
    }

    /// <summary>
    /// Orders paths deepest-first so batch moves do not break nested sources listed together.
    /// </summary>
    public static IReadOnlyList<string> OrderForBatchMove(IEnumerable<string> sourcePaths) =>
        sourcePaths
            .OrderByDescending(NormalizeKey, StringComparer.Ordinal)
            .ToList();

    public static string ReplacePrefix(string key, string sourcePrefix, string destinationPrefix)
    {
        var normalizedKey = key.Replace('\\', '/');
        var normalizedSource = NormalizeKey(sourcePrefix);
        var normalizedDestination = NormalizeKey(destinationPrefix);

        if (normalizedKey.Equals(normalizedSource, StringComparison.Ordinal))
            return normalizedDestination;

        var sourceWithSlash = normalizedSource + "/";
        if (normalizedKey.StartsWith(sourceWithSlash, StringComparison.Ordinal))
            return normalizedDestination + normalizedKey[normalizedSource.Length..];

        return normalizedKey;
    }

    public static IEnumerable<string> CollectKeysUnderPrefix(
        IEnumerable<string> keys,
        string prefix,
        bool includeExactMatch = true)
    {
        var normalized = NormalizeKey(prefix);
        var prefixWithSlash = normalized + "/";

        foreach (var key in keys)
        {
            if (key.Equals(normalized, StringComparison.Ordinal) && includeExactMatch)
            {
                yield return key;
                continue;
            }

            if (key.StartsWith(prefixWithSlash, StringComparison.Ordinal))
                yield return key;
        }
    }
}
