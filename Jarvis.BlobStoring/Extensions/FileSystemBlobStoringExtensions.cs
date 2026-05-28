// Jarvis.BlobStoring — DI registration for FileSystem blob provider.
using Jarvis.BlobStoring.Configuration;
using Jarvis.BlobStoring.FileSystem;
using Jarvis.BlobStoring.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jarvis.BlobStoring.Extensions;

public static class FileSystemBlobStoringExtensions
{
    public static BlobStoringBuilder UseFileSystem(this BlobStoringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var section = $"{JarvisBlobStoringOptions.SectionName}:FileSystem";
        var snapshot = new FileSystemOption();
        builder.HostBuilder.Configuration.GetSection(section).Bind(snapshot);

        builder.HostBuilder.Services
            .AddOptions<FileSystemOption>()
            .BindConfiguration(section)
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.RootPath))
                    options.RootPath = Path.Combine(
                        builder.HostBuilder.Environment.ContentRootPath,
                        "wwwroot",
                        "blobs");
            });

        builder.HostBuilder.Services.TryAddKeyedSingleton<IBlobStoringService, FileSystemBlobStoringService>(
            nameof(BlobStoringType.FileSystem));

        builder.HostBuilder.Services
            .GetOrAddProviderRegistry()
            .Register(nameof(BlobStoringType.FileSystem), FileSystemBlobStoringDefaults.ResolveAutoSelectPriority(snapshot));

        return builder;
    }
}
