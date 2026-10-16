using Platform.BlobStoring;
using Platform.BlobStoring.Synology;
using Platform.Modules.Files.Models;
using Platform.Modules.Files.Storage;

namespace Platform.Modules.Files.API.Storage;

internal sealed class SynologyFileTreeProvider(ISynologyFolderService folderService) : IFileTreeProvider
{
    public string ProviderKey => nameof(BlobStoringType.Synology);

    public async Task<FileTreeNode> GetTreeAsync(
        string? bucket = null,
        string? rootPath = null,
        CancellationToken cancellationToken = default)
    {
        var tree = await folderService.GetTreeAsync(bucket, cancellationToken).ConfigureAwait(false);
        return Map(tree);
    }

    private static FileTreeNode Map(SynologyDriveNode node) => new()
    {
        Name = node.Name,
        Path = node.Path,
        IsFolder = node.IsFolder,
        Children = node.Children.Select(Map).ToList()
    };
}
