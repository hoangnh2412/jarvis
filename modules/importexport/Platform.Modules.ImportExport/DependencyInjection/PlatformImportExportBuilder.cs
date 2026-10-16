using Microsoft.Extensions.Hosting;
using Platform.Modules.ImportExport.Application.Schemas;

namespace Platform.Modules.ImportExport.DependencyInjection;

/// <summary>
/// Builder fluent sau <see cref="PlatformImportExportExtensions.AddCoreImport"/> /
/// <see cref="PlatformImportExportExtensions.AddCoreExport"/> /
/// <see cref="PlatformImportExportExtensions.AddCoreImportExport"/>.
/// </summary>
public sealed class PlatformImportExportBuilder
{
    internal PlatformImportExportBuilder(IHostApplicationBuilder hostBuilder)
    {
        HostBuilder = hostBuilder;
    }

    public IHostApplicationBuilder HostBuilder { get; }

    public PlatformImportExportBuilder ConfigureEntities(Action<ImportExportEntityRegistryOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        HostBuilder.Services.ConfigureImportExportEntities(configure);
        return this;
    }
}
