// Platform.Modules.Files — Host DI: registers IFileManager (blob wiring stays in host).
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Platform.Modules.Files.Services;
using Platform.Modules.Files.Storage;

namespace Platform.Modules.Files.Extensions;

/// <summary>Registers the Files module core services into the host.</summary>
public static class PlatformFilesExtensions
{
    /// <summary>
    /// Registers <see cref="IFileManager"/> and storage resolver.
    /// Host must call <c>AddCoreBlobStoring()</c> (and optional <c>Use*</c> providers) before or after this method.
    /// Opt-in HTTP API: reference <c>Platform.Modules.Files.API</c> and chain <c>UseHttpApi()</c>.
    /// </summary>
    public static PlatformFilesBuilder AddCoreFiles(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddSingleton<IFileStorageResolver, DefaultFileStorageResolver>();
        builder.Services.TryAddScoped<IFileManager, FileManager>();

        return new PlatformFilesBuilder(builder);
    }
}
