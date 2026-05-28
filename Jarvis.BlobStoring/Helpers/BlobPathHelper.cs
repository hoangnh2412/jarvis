// Jarvis.BlobStoring — Safe path combine for FileSystem blob storage (prevents directory traversal).
namespace Jarvis.BlobStoring.Helpers;

public static class BlobPathHelper
{
    /// <summary>
    /// Builds an absolute path under <paramref name="rootPath"/> for FileSystem blob storage.
    /// Rejects <c>..</c> in segments and ensures the result cannot escape the configured root.
    /// </summary>
    /// <param name="rootPath">Configured storage root, e.g. <c>D:/data/blobs</c>.</param>
    /// <param name="subPath">Optional folder under root; may be empty.</param>
    /// <param name="bucket">Logical bucket name (folder under base path).</param>
    /// <param name="fileName">Object key; pass empty to resolve the bucket directory only (listing).</param>
    /// <returns>Absolute normalized path on disk.</returns>
    /// <exception cref="InvalidOperationException">Traversal detected or resolved path escapes root.</exception>
    /// <remarks>
    /// <para><b>Valid upload/download</b></para>
    /// <code>
    /// Combine("D:/data/blobs", "", "invoices", "2024/inv-001.pdf")
    /// // → D:\data\blobs\invoices\2024\inv-001.pdf
    /// </code>
    /// <para><b>Listing (empty fileName)</b></para>
    /// <code>
    /// Combine("D:/data/blobs", "tenant-a", "invoices", "")
    /// // → D:\data\blobs\tenant-a\invoices
    /// </code>
    /// <para><b>Rejected — explicit <c>..</c> in fileName</b></para>
    /// <code>
    /// Combine("D:/data/blobs", "", "invoices", "../secrets.txt")  // throws
    /// </code>
    /// <para><b>Rejected — path escapes root after normalization</b></para>
    /// <code>
    /// // Even if ".." were not caught earlier, StartsWith(basePath) blocks escape.
    /// </code>
    /// </remarks>
    public static string Combine(string rootPath, string subPath, string bucket, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(bucket);

        // e.g. fileName "../secrets.txt" → throw before touching disk
        if (ContainsTraversal(rootPath) || ContainsTraversal(subPath) || ContainsTraversal(bucket))
            throw new InvalidOperationException("Blob path segments must not contain '..'.");

        if (!string.IsNullOrWhiteSpace(fileName) && ContainsTraversal(fileName))
            throw new InvalidOperationException("Blob path segments must not contain '..'.");

        // basePath = GetFullPath("D:/data/blobs") or GetFullPath("D:/data/blobs/tenant-a")
        var basePath = string.IsNullOrWhiteSpace(subPath)
            ? Path.GetFullPath(rootPath)
            : Path.GetFullPath(Path.Combine(rootPath, subPath));

        var fullPath = string.IsNullOrWhiteSpace(fileName)
            ? Path.GetFullPath(Path.Combine(basePath, bucket))
            : Path.GetFullPath(Path.Combine(basePath, bucket, fileName));

        // e.g. fullPath must stay under D:\data\blobs\tenant-a\invoices
        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Resolved blob path is outside the configured root.");

        return fullPath;
    }

    /// <summary>
    /// Detects <c>..</c> in a path segment.
    /// </summary>
    /// <example><c>../etc/passwd</c>, <c>foo..bar</c> (substring match).</example>
    private static bool ContainsTraversal(string segment) => segment.Contains("..", StringComparison.Ordinal);
}
