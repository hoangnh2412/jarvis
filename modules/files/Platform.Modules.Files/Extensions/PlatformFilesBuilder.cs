using Microsoft.Extensions.Hosting;

namespace Platform.Modules.Files.Extensions;

/// <summary>Fluent follow-up after <see cref="PlatformFilesExtensions.AddCoreFiles"/>.</summary>
public sealed class PlatformFilesBuilder
{
    internal PlatformFilesBuilder(IHostApplicationBuilder hostBuilder)
    {
        HostBuilder = hostBuilder;
    }

    public IHostApplicationBuilder HostBuilder { get; }
}
