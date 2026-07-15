// Jarvis.BlobStoring.MinIO — DI registration for MinIO blob provider.
using Jarvis.BlobStoring.Configuration;
using Jarvis.BlobStoring.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jarvis.BlobStoring.MinIO.Extensions;

public static class MinIOBlobStoringExtensions
{
    public static BlobStoringBuilder UseMinIO(
        this BlobStoringBuilder builder,
        Action<MinIOBlobStoringOption>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var section = $"{JarvisBlobStoringOptions.SectionName}:{MinIOBlobStoringDefaults.ConfigurationSection}";
        var snapshot = new MinIOBlobStoringOption();
        builder.HostBuilder.Configuration.GetSection(section).Bind(snapshot);
        configure?.Invoke(snapshot);

        if (!MinIOBlobStoringDefaults.IsEnabled(snapshot))
            throw new InvalidOperationException($"BlobStoring:{MinIOBlobStoringDefaults.ConfigurationSection}:Endpoint is required for {nameof(UseMinIO)}.");

        builder.HostBuilder.Services
            .AddOptions<MinIOBlobStoringOption>()
            .BindConfiguration(section);

        if (configure is not null)
            builder.HostBuilder.Services.Configure(configure);

        builder.HostBuilder.Services.TryAddKeyedSingleton<IBlobStoringService, MinioBlobStoringService>(nameof(BlobStoringType.MinIO));

        builder.HostBuilder.Services
            .GetOrAddProviderRegistry()
            .Register(nameof(BlobStoringType.MinIO), MinIOBlobStoringDefaults.ResolveAutoSelectPriority(snapshot));

        return builder;
    }
}
