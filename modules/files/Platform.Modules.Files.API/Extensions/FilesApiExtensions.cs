using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Modules.Files.API.Controllers;
using Platform.Modules.Files.API.Storage;
using Platform.Modules.Files.Extensions;
using Platform.Modules.Files.Storage;

namespace Platform.Modules.Files.API.Extensions;

/// <summary>Opt-in HTTP API registration for the Files module.</summary>
public static class FilesApiExtensions
{
    /// <summary>
    /// Expose file controllers at <c>api/v{version}/files</c>.
    /// Call after <c>AddCoreFiles()</c> on the same builder chain.
    /// </summary>
    public static PlatformFilesBuilder UseHttpApi(this PlatformFilesBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HostBuilder.Services
            .AddControllers()
            .AddApplicationPart(typeof(FileController).Assembly);

        return builder;
    }

    /// <summary>
    /// Registers SharePoint folder tree browse when <c>UseSharePoint()</c> blob provider is active.
    /// </summary>
    public static PlatformFilesBuilder UseSharePointTree(this PlatformFilesBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HostBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IFileTreeProvider, SharePointFileTreeProvider>());

        return builder;
    }

    /// <summary>
    /// Registers Synology folder tree browse when <c>UseSynology()</c> blob provider is active.
    /// </summary>
    public static PlatformFilesBuilder UseSynologyTree(this PlatformFilesBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HostBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IFileTreeProvider, SynologyFileTreeProvider>());

        return builder;
    }

    /// <summary>
    /// Registers FileSystem folder tree browse (lazy, one level per <c>rootPath</c> query).
    /// </summary>
    public static PlatformFilesBuilder UseFileSystemTree(this PlatformFilesBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HostBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IFileTreeProvider, FileSystemFileTreeProvider>());

        return builder;
    }
}
