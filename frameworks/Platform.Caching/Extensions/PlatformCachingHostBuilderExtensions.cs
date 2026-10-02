// Platform.Caching — Host DI: binds Cache options, registers memory + ICacheService, optional Redis via extensions.
using Platform.Caching.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Platform.Caching.Extensions;

/// <summary>
/// Registers Platform multi-tier caching (memory default, optional distributed layers).
/// </summary>
public static class PlatformCachingHostBuilderExtensions
{
    /// <summary>
    /// Binds <see cref="PlatformCacheOptions"/> from configuration and registers <see cref="ICacheService"/>.
    /// </summary>
    public static PlatformCachingBuilder AddPlatformCaching(
        this IHostApplicationBuilder builder,
        Action<PlatformCacheOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services
            .AddOptions<PlatformCacheOptions>()
            .BindConfiguration(PlatformCacheOptions.SectionName);

        if (configure is not null)
            builder.Services.Configure(configure);

        builder.Services.AddMemoryCache();
        builder.Services.TryAddSingleton<DistributedCacheRegistry>(sp =>
        {
            var registry = new DistributedCacheRegistry();
            foreach (var configurator in sp.GetServices<IConfigureOptions<DistributedCacheRegistry>>())
                configurator.Configure(registry);

            return registry;
        });
        builder.Services.TryAddSingleton<IMemoryCache>(sp =>
            new MsMemoryCacheAdapter(sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>()));

        builder.Services.TryAddSingleton<ICacheService>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PlatformCacheOptions>>().Value;
            var cacheService = new CacheService(options);
            cacheService.SetServiceProvider(sp);
            cacheService.SetMemCache(sp.GetRequiredService<IMemoryCache>());
            cacheService.SetDistributedCacheRegistry(sp.GetRequiredService<DistributedCacheRegistry>());
            return cacheService;
        });

        var snapshot = new PlatformCacheOptions();
        builder.Configuration.GetSection(PlatformCacheOptions.SectionName).Bind(snapshot);
        configure?.Invoke(snapshot);

        return new PlatformCachingBuilder(builder, snapshot);
    }
}

/// <summary>
/// Fluent follow-up after <see cref="PlatformCachingHostBuilderExtensions.AddPlatformCaching"/>.
/// </summary>
public sealed class PlatformCachingBuilder
{
    internal PlatformCachingBuilder(IHostApplicationBuilder hostBuilder, PlatformCacheOptions optionsSnapshot)
    {
        HostBuilder = hostBuilder;
        OptionsSnapshot = optionsSnapshot;
    }

    public IHostApplicationBuilder HostBuilder { get; }

    public PlatformCacheOptions OptionsSnapshot { get; }
}
