namespace Platform.BlobStoring.Synology;

/// <summary>
/// Opt-in browse API for Synology Drive (not part of <see cref="IBlobStoringService"/>).
/// </summary>
public interface ISynologyFolderService
{
    /// <summary>
    /// Lazy tree: liệt kê con trực tiếp dưới <paramref name="bucket"/>; folder con có <c>Children</c> rỗng.
    /// Missing path → root node with empty children.
    /// </summary>
    Task<SynologyDriveNode> GetTreeAsync(
        string? bucket = null,
        CancellationToken cancellationToken = default);
}

/// <summary>Tree node; path is relative to <see cref="SynologyBlobOptions.RootPath"/>.</summary>
public sealed class SynologyDriveNode
{
    public required string Name { get; init; }

    public required string Path { get; init; }

    public bool IsFolder { get; init; }

    public IReadOnlyList<SynologyDriveNode> Children { get; init; } = [];
}
