namespace Platform.Modules.Files.Models;

/// <summary>Input for rename validation and manager calls.</summary>
public sealed class RenameFileInput
{
    public string Path { get; init; } = string.Empty;

    public string NewName { get; init; } = string.Empty;
}
