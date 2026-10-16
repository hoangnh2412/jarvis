// Platform.BlobStoring.Synology — DI registration (same shape as MinIO / SharePoint).
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.BlobStoring.Configuration;
using Platform.BlobStoring.Hosting;

namespace Platform.BlobStoring.Synology.Extensions;

public static class SynologyBlobStoringExtensions
{
    public static BlobStoringBuilder UseSynology(
        this BlobStoringBuilder builder,
        Action<SynologyBlobOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var section = $"{PlatformBlobStoringOptions.SectionName}:{SynologyBlobStoringDefaults.ConfigurationSection}";
        var snapshot = new SynologyBlobOptions();
        builder.HostBuilder.Configuration.GetSection(section).Bind(snapshot);
        configure?.Invoke(snapshot);

        if (!SynologyBlobStoringDefaults.IsEnabled(snapshot))
        {
            throw new InvalidOperationException(
                $"BlobStoring:{SynologyBlobStoringDefaults.ConfigurationSection}:Host, Username, and Password are required for {nameof(UseSynology)}.");
        }

        builder.HostBuilder.Services
            .AddOptions<SynologyBlobOptions>()
            .BindConfiguration(section);

        if (configure is not null)
            builder.HostBuilder.Services.Configure(configure);

        builder.HostBuilder.Services.TryAddKeyedSingleton<IBlobStoringService, SynologyBlobStoringService>(
            nameof(BlobStoringType.Synology));

        builder.HostBuilder.Services.TryAddSingleton<ISynologyFolderService>(sp =>
            (ISynologyFolderService)sp.GetRequiredKeyedService<IBlobStoringService>(
                nameof(BlobStoringType.Synology)));

        builder.HostBuilder.Services
            .GetOrAddProviderRegistry()
            .Register(nameof(BlobStoringType.Synology), SynologyBlobStoringDefaults.ResolveAutoSelectPriority(snapshot));

        return builder;
    }
}
