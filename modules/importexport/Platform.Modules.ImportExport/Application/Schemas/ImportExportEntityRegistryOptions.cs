using Platform.Modules.ImportExport.Application.Schemas.Models;

namespace Platform.Modules.ImportExport.Application.Schemas;

public sealed class ImportExportEntityRegistryOptions
{
    internal List<RegisteredImportExportEntity> Entities { get; } = [];

    public ImportExportEntityRegistryOptions Add<TRow>(string key, string displayName)
        where TRow : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        if (Entities.Any(e => e.Key.Equals(key, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Duplicate import-export entity key '{key}'.");

        Entities.Add(new RegisteredImportExportEntity(key.Trim(), typeof(TRow), displayName.Trim()));
        return this;
    }
}
