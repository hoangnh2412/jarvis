// Platform.BlobStoring — Fluent builder returned by AddBlobStoring.
using Platform.BlobStoring.Configuration;
using Microsoft.Extensions.Hosting;

namespace Platform.BlobStoring.Hosting;

/// <summary>
/// Fluent follow-up after <see cref="Extensions.BlobStoringHostBuilderExtensions.AddCoreBlobStoring"/>.
/// </summary>
public sealed class BlobStoringBuilder
{
    internal BlobStoringBuilder(IHostApplicationBuilder hostBuilder, PlatformBlobStoringOptions optionsSnapshot)
    {
        HostBuilder = hostBuilder;
        OptionsSnapshot = optionsSnapshot;
    }

    public IHostApplicationBuilder HostBuilder { get; }

    public PlatformBlobStoringOptions OptionsSnapshot { get; }
}
