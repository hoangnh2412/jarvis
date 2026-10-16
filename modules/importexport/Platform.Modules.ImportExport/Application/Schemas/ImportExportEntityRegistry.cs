using Microsoft.Extensions.Options;
using Platform.Modules.ImportExport.Application.Schemas.Models;

namespace Platform.Modules.ImportExport.Application.Schemas;

public sealed class ImportExportEntityRegistry : IImportExportEntityRegistry
{
    private readonly Dictionary<string, RegisteredImportExportEntity> _map;

    public ImportExportEntityRegistry(IOptions<ImportExportEntityRegistryOptions> options)
    {
        _map = options.Value.Entities
            .ToDictionary(e => e.Key, e => e, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ImportExportEntitySummary> List() =>
        _map.Values
            .OrderBy(e => e.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(e => new ImportExportEntitySummary(e.Key, e.DisplayName))
            .ToList();

    public bool TryGet(string key, out RegisteredImportExportEntity entity) =>
        _map.TryGetValue(key.Trim(), out entity!);
}
