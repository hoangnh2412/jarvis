using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Modules.ImportExport.Application.Import;

namespace Platform.Modules.ImportExport.DependencyInjection;

/// <summary>
/// Keyed module persister registration for import persist routing.
/// </summary>
public static class ImportModulePersisterExtensions
{
    /// <summary>
    /// Registers keyed module persister and ensures composite is the single <see cref="IImportDataPersister"/>.
    /// Call once per module entity key from the host (before or after <c>UseEntityFramework</c>).
    /// </summary>
    public static IServiceCollection AddImportModulePersister<T>(
        this IServiceCollection services,
        string entityKey)
        where T : class, IImportModulePersister
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityKey);
        var trimmedKey = entityKey.Trim();
        services.AddKeyedScoped<IImportModulePersister, T>(trimmedKey);
        services.AddScoped<IImportModulePersister>(sp =>
            sp.GetRequiredKeyedService<IImportModulePersister>(trimmedKey));
        services.TryAddScoped<IImportDataPersister, ImportModulePersisterComposite>();
        return services;
    }
}
