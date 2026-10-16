namespace Platform.Modules.Files.API.Models;

public sealed class MoveBatchFileRequest
{
    /// <summary>Target folder relative to bucket. Empty = bucket root.</summary>
    public string DestinationFolder { get; init; } = string.Empty;

    public string[] SourcePaths { get; init; } = [];
}
