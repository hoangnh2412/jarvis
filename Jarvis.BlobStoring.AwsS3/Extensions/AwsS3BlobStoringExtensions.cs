// Jarvis.BlobStoring.AwsS3 — DI registration for AWS S3 blob provider.
using Jarvis.BlobStoring.Configuration;
using Jarvis.BlobStoring.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jarvis.BlobStoring.AwsS3.Extensions;

public static class AwsS3BlobStoringExtensions
{
    public static BlobStoringBuilder UseAwsS3(
        this BlobStoringBuilder builder,
        Action<AwsS3BlobOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var section = $"{JarvisBlobStoringOptions.SectionName}:{AwsS3BlobStoringDefaults.ConfigurationSection}";
        var snapshot = new AwsS3BlobOptions();
        builder.HostBuilder.Configuration.GetSection(section).Bind(snapshot);
        configure?.Invoke(snapshot);

        if (!AwsS3BlobStoringDefaults.IsEnabled(snapshot))
            throw new InvalidOperationException($"BlobStoring:{AwsS3BlobStoringDefaults.ConfigurationSection}:Region and BucketName are required for {nameof(UseAwsS3)}.");

        builder.HostBuilder.Services
            .AddOptions<AwsS3BlobOptions>()
            .BindConfiguration(section);

        if (configure is not null)
            builder.HostBuilder.Services.Configure(configure);

        builder.HostBuilder.Services.TryAddKeyedSingleton<IBlobStoringService, AwsS3BlobStoringService>(
            nameof(BlobStoringType.AwsS3));

        builder.HostBuilder.Services
            .GetOrAddProviderRegistry()
            .Register(nameof(BlobStoringType.AwsS3), AwsS3BlobStoringDefaults.ResolveAutoSelectPriority(snapshot));

        return builder;
    }
}
