namespace Platform.Modules.Files.Models;

/// <summary>Tree node for folder browse API (provider-agnostic DTO).</summary>
public sealed class FileTreeNode
{
    public required string Name { get; init; }

    public required string Path { get; init; }

    public bool IsFolder { get; init; }

    public IReadOnlyList<FileTreeNode> Children { get; init; } = [];
}
