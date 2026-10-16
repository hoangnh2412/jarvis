namespace Platform.Modules.Files.Models;

/// <summary>Input for single-item move validation and manager calls.</summary>
public sealed class MoveFileInput
{
    public string SourcePath { get; init; } = string.Empty;

    /// <summary>Target folder relative to bucket. Empty = bucket root.</summary>
    public string DestinationFolder { get; init; } = string.Empty;
}
