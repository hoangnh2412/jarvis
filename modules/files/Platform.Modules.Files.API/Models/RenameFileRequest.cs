namespace Platform.Modules.Files.API.Models;

public sealed class RenameFileRequest
{
    public string Path { get; init; } = string.Empty;

    public string NewName { get; init; } = string.Empty;
}
