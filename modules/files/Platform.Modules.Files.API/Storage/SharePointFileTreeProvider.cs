using Platform.BlobStoring;
using Platform.BlobStoring.SharePoint;
using Platform.Modules.Files.Models;
using Platform.Modules.Files.Storage;

namespace Platform.Modules.Files.API.Storage;

internal sealed class SharePointFileTreeProvider(ISharePointFolderService folderService) : IFileTreeProvider
{
    public string ProviderKey => nameof(BlobStoringType.SharePoint);

    public async Task<FileTreeNode> GetTreeAsync(
        string? bucket = null,
        string? rootPath = null,
        CancellationToken cancellationToken = default)
    {
        var root = string.IsNullOrWhiteSpace(rootPath) ? bucket : rootPath;
        var tree = await folderService.GetTreeAsync(root, cancellationToken).ConfigureAwait(false);
        return Map(tree);
    }

    private static FileTreeNode Map(SharePointDriveNode node) => new()
    {
        Name = node.Name,
        Path = node.Path,
        IsFolder = node.IsFolder,
        Children = node.Children.Select(Map).ToList()
    };
}
