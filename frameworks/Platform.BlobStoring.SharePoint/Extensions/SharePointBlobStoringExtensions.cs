using Platform.BlobStoring.Configuration;
using Platform.BlobStoring.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Platform.BlobStoring.SharePoint.Extensions;

/// <summary>
/// Registers the SharePoint blob storing provider (same pattern as MinIO / AwsS3).
/// </summary>
public static class SharePointBlobStoringExtensions
{
    public static BlobStoringBuilder UseSharePoint(
        this BlobStoringBuilder builder,
        Action<SharePointBlobOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var section = $"{PlatformBlobStoringOptions.SectionName}:{SharePointBlobStoringDefaults.ConfigurationSection}";
        var snapshot = new SharePointBlobOptions();
        builder.HostBuilder.Configuration.GetSection(section).Bind(snapshot);
        configure?.Invoke(snapshot);

        if (!SharePointBlobStoringDefaults.IsEnabled(snapshot))
        {
            throw new InvalidOperationException(
                $"BlobStoring:{SharePointBlobStoringDefaults.ConfigurationSection}:TenantId, ClientId, ClientSecret, and DriveId are required for {nameof(UseSharePoint)}.");
        }

        builder.HostBuilder.Services
            .AddOptions<SharePointBlobOptions>()
            .BindConfiguration(section);

        if (configure is not null)
            builder.HostBuilder.Services.Configure(configure);

        builder.HostBuilder.Services.TryAddKeyedSingleton<IBlobStoringService, SharePointBlobStoringService>(
            nameof(BlobStoringType.SharePoint));

        builder.HostBuilder.Services.TryAddSingleton<ISharePointFolderService>(sp =>
            (ISharePointFolderService)sp.GetRequiredKeyedService<IBlobStoringService>(
                nameof(BlobStoringType.SharePoint)));

        builder.HostBuilder.Services
            .GetOrAddProviderRegistry()
            .Register(nameof(BlobStoringType.SharePoint), SharePointBlobStoringDefaults.ResolveAutoSelectPriority(snapshot));

        return builder;
    }
}
