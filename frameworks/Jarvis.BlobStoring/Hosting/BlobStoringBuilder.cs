// Jarvis.BlobStoring — Fluent builder returned by AddBlobStoring.
using Jarvis.BlobStoring.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jarvis.BlobStoring.Hosting;

/// <summary>
/// Fluent follow-up after <see cref="Extensions.BlobStoringHostBuilderExtensions.AddCoreBlobStoring"/>.
/// </summary>
public sealed class BlobStoringBuilder
{
    internal BlobStoringBuilder(IHostApplicationBuilder hostBuilder, JarvisBlobStoringOptions optionsSnapshot)
    {
        HostBuilder = hostBuilder;
        OptionsSnapshot = optionsSnapshot;
    }

    public IHostApplicationBuilder HostBuilder { get; }

    public JarvisBlobStoringOptions OptionsSnapshot { get; }
}
