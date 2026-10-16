namespace Platform.Modules.Files.API.Models;

public sealed class MoveFileRequest
{
    public string SourcePath { get; init; } = string.Empty;

    /// <summary>Target folder relative to bucket. Empty = bucket root.</summary>
    public string DestinationFolder { get; init; } = string.Empty;
}
