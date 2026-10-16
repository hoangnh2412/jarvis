// Platform.BlobStoring — Safe path combine for FileSystem blob storage (prevents directory traversal).
namespace Platform.BlobStoring.Helpers;

public static class BlobPathHelper
{
    /// <summary>
    /// Builds an absolute path under <paramref name="rootPath"/> for FileSystem blob storage.
    /// Rejects <c>.</c> / <c>..</c> path segments and ensures the result cannot escape the configured root.
    /// When <paramref name="bucket"/> is empty, the path is resolved directly under root + subPath.
    /// </summary>
    public static string Combine(string rootPath, string subPath, string? bucket, string? fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);

        if (ContainsTraversalSegment(rootPath)
            || ContainsTraversalSegment(subPath)
            || (!string.IsNullOrWhiteSpace(bucket) && ContainsTraversalSegment(bucket))
            || (!string.IsNullOrWhiteSpace(fileName) && ContainsTraversalSegment(fileName)))
        {
            throw new InvalidOperationException("Blob path segments must not contain '.' or '..'.");
        }

        var basePath = string.IsNullOrWhiteSpace(subPath)
            ? Path.GetFullPath(rootPath)
            : Path.GetFullPath(Path.Combine(rootPath, subPath));

        string fullPath;
        if (string.IsNullOrWhiteSpace(bucket))
        {
            fullPath = string.IsNullOrWhiteSpace(fileName)
                ? basePath
                : Path.GetFullPath(Path.Combine(basePath, fileName));
        }
        else
        {
            fullPath = string.IsNullOrWhiteSpace(fileName)
                ? Path.GetFullPath(Path.Combine(basePath, bucket))
                : Path.GetFullPath(Path.Combine(basePath, bucket, fileName));
        }

        if (!IsUnderRoot(fullPath, basePath))
            throw new InvalidOperationException("Resolved blob path is outside the configured root.");

        return fullPath;
    }

    /// <summary>
    /// Returns true when <paramref name="path"/> contains a <c>.</c> or <c>..</c> path segment.
    /// Legitimate names such as <c>report..final.pdf</c> are allowed.
    /// </summary>
    public static bool ContainsTraversalSegment(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return path.Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Any(static segment => segment is "." or "..");
    }

    /// <summary>
    /// Ensures <paramref name="fullPath"/> is the same as or nested under <paramref name="rootPath"/>.
    /// </summary>
    internal static bool IsUnderRoot(string fullPath, string rootPath)
    {
        var normalizedRoot = Path.GetFullPath(rootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        var normalizedFull = Path.GetFullPath(fullPath);

        if (string.Equals(normalizedFull, normalizedRoot, StringComparison.OrdinalIgnoreCase))
            return true;

        var rootPrefix = normalizedRoot + Path.DirectorySeparatorChar;
        return normalizedFull.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase);
    }
}
