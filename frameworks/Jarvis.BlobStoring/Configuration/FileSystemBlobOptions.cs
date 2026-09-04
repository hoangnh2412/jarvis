// Jarvis.BlobStoring — FileSystem section under BlobStoring:FileSystem.
namespace Jarvis.BlobStoring.Configuration;

/// <summary>
/// FileSystem provider settings under <c>BlobStoring:FileSystem</c>.
/// Empty <see cref="RootPath"/> at runtime falls back to <c>{ContentRoot}/wwwroot/blobs</c> (see <c>UseFileSystem</c>).
/// </summary>
/// <remarks>
/// <example>
/// <code>
/// {
///   "BlobStoring": {
///     "FileSystem": {
///       "RootPath": "D:/data/blobs",
///       "SubPath": "tenant-a",
///       "AutoSelectPriority": 10
///     }
///   }
/// }
/// // → files stored under D:\data\blobs\tenant-a\{bucket}\{fileName}
/// </code>
/// </example>
/// </remarks>
public class FileSystemBlobOptions
{
    /// <summary>Absolute or relative root directory on disk.</summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>Optional subfolder under <see cref="RootPath"/> (e.g. per tenant).</summary>
    public string SubPath { get; set; } = string.Empty;

    /// <summary>Auto-default priority when <c>DefaultProvider</c> is empty. Higher wins. 0 = use built-in default (10).</summary>
    public int AutoSelectPriority { get; set; }
}
