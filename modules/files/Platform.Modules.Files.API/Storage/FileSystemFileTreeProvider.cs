using Microsoft.Extensions.Options;
using Platform.BlobStoring;
using Platform.BlobStoring.Configuration;
using Platform.BlobStoring.Helpers;
using Platform.Modules.Files.Models;
using Platform.Modules.Files.Storage;

namespace Platform.Modules.Files.API.Storage;

/// <summary>
/// Lazy folder tree for the FileSystem blob provider — lists one directory level per request.
/// </summary>
internal sealed class FileSystemFileTreeProvider(IOptions<FileSystemBlobOptions> options) : IFileTreeProvider
{
    private readonly FileSystemBlobOptions _options = options.Value;

    public string ProviderKey => nameof(BlobStoringType.FileSystem);

    public Task<FileTreeNode> GetTreeAsync(
        string? bucket = null,
        string? rootPath = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var relativePath = NormalizeRelativePath(rootPath);
        if (BlobPathHelper.ContainsTraversalSegment(relativePath))
            throw new InvalidOperationException("Tree root path must not contain '.' or '..' segments.");

        var bucketSegment = NormalizeBucket(bucket);
        if (BlobPathHelper.ContainsTraversalSegment(bucketSegment))
            throw new InvalidOperationException("Tree bucket must not contain '.' or '..' segments.");

        var directory = ResolveDirectory(bucketSegment, relativePath);
        if (!Directory.Exists(directory))
        {
            return Task.FromResult(CreateNode(relativePath, bucketSegment, []));
        }

        var children = new List<FileTreeNode>();

        foreach (var subDirectory in Directory.EnumerateDirectories(directory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = Path.GetFileName(subDirectory);
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var childPath = CombineRelativePath(relativePath, name);
            children.Add(new FileTreeNode
            {
                Name = name,
                Path = childPath,
                IsFolder = true,
                Children = [],
            });
        }

        foreach (var file in Directory.EnumerateFiles(directory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = Path.GetFileName(file);
            if (string.IsNullOrWhiteSpace(name))
                continue;

            children.Add(new FileTreeNode
            {
                Name = name,
                Path = CombineRelativePath(relativePath, name),
                IsFolder = false,
                Children = [],
            });
        }

        children.Sort(static (a, b) =>
            string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(CreateNode(relativePath, bucketSegment, children));
    }

    private string ResolveDirectory(string bucket, string relativePath)
    {
        var bucketValue = string.IsNullOrEmpty(bucket) ? null : bucket;
        var pathValue = string.IsNullOrEmpty(relativePath) ? null : relativePath;
        return BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucketValue, pathValue);
    }

    private static FileTreeNode CreateNode(
        string relativePath,
        string bucket,
        IReadOnlyList<FileTreeNode> children)
    {
        var name = string.IsNullOrEmpty(relativePath)
            ? string.IsNullOrEmpty(bucket) ? string.Empty : bucket
            : relativePath.Split('/').Last();

        return new FileTreeNode
        {
            Name = name,
            Path = relativePath,
            IsFolder = true,
            Children = children,
        };
    }

    private static string NormalizeRelativePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        return path.Trim().Trim('/').Replace('\\', '/');
    }

    private static string NormalizeBucket(string? bucket)
    {
        if (string.IsNullOrWhiteSpace(bucket))
            return string.Empty;

        return bucket.Trim().Trim('/');
    }

    private static string CombineRelativePath(string parent, string name)
    {
        return string.IsNullOrEmpty(parent) ? name : $"{parent}/{name}";
    }
}
