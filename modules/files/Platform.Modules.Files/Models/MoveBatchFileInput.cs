namespace Platform.Modules.Files.Models;

/// <summary>Input for batch move validation and manager calls.</summary>
public sealed class MoveBatchFileInput
{
    /// <summary>Target folder relative to bucket. Empty = bucket root.</summary>
    public string DestinationFolder { get; init; } = string.Empty;

    public string[] SourcePaths { get; init; } = [];
}
