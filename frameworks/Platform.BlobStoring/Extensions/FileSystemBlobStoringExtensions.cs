// Platform.BlobStoring — DI registration for FileSystem blob provider.
using Platform.BlobStoring.Configuration;
using Platform.BlobStoring.FileSystem;
using Platform.BlobStoring.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Platform.BlobStoring.Extensions;

public static class FileSystemBlobStoringExtensions
{
    public static BlobStoringBuilder UseFileSystem(
        this BlobStoringBuilder builder,
        Action<FileSystemBlobOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var section = $"{PlatformBlobStoringOptions.SectionName}:FileSystem";
        var snapshot = new FileSystemBlobOptions();
        builder.HostBuilder.Configuration.GetSection(section).Bind(snapshot);
        configure?.Invoke(snapshot);

        builder.HostBuilder.Services
            .AddOptions<FileSystemBlobOptions>()
            .BindConfiguration(section)
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.RootPath))
                    options.RootPath = Path.Combine(
                        builder.HostBuilder.Environment.ContentRootPath,
                        "wwwroot",
                        "blobs");
            });

        if (configure is not null)
            builder.HostBuilder.Services.Configure(configure);

        builder.HostBuilder.Services.TryAddKeyedSingleton<IBlobStoringService, FileSystemBlobStoringService>(
            nameof(BlobStoringType.FileSystem));

        builder.HostBuilder.Services
            .GetOrAddProviderRegistry()
            .Register(
                nameof(BlobStoringType.FileSystem),
                FileSystemBlobStoringDefaults.ResolveAutoSelectPriority(snapshot));

        return builder;
    }
}
