using Platform.Modules.Files.Models;

namespace Platform.Modules.Files.Storage;

/// <summary>
/// Optional provider-specific folder tree browse (SharePoint, Synology, …).
/// Register via <c>UseSharePointTree</c> / <c>UseSynologyTree</c> / <c>UseFileSystemTree</c> on the API builder.
/// </summary>
public interface IFileTreeProvider
{
    /// <summary>Matches <see cref="Platform.BlobStoring.BlobStoringType"/> name (e.g. SharePoint, Synology).</summary>
    string ProviderKey { get; }

    /// <summary>
    /// Lazy folder tree: liệt kê con trực tiếp dưới <paramref name="rootPath"/> (<paramref name="bucket"/>).
    /// Gọi lại với <paramref name="rootPath"/> = path folder con để tải cấp tiếp theo.
    /// </summary>
    Task<FileTreeNode> GetTreeAsync(
        string? bucket = null,
        string? rootPath = null,
        CancellationToken cancellationToken = default);
}
