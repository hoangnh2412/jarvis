namespace Platform.Modules.ImportExport.Application.Import.Models;

public sealed class ImportPersistResult
{
    public int SavedCount { get; init; }
    public IReadOnlyList<ImportRowError> Errors { get; init; } = [];
}
