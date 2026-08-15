// Jarvis.Caching — Host DI: binds Cache options, registers memory + ICacheService, optional Redis via extensions.
using Jarvis.Caching.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jarvis.Caching.Extensions;

/// <summary>
/// Registers Jarvis multi-tier caching (memory default, optional distributed layers).
/// </summary>
public static class JarvisCachingHostBuilderExtensions
{
    /// <summary>
    /// Binds <see cref="JarvisCacheOptions"/> from configuration and registers <see cref="ICacheService"/>.
    /// </summary>
    public static JarvisCachingBuilder AddJarvisCaching(
        this IHostApplicationBuilder builder,
        Action<JarvisCacheOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services
            .AddOptions<JarvisCacheOptions>()
            .BindConfiguration(JarvisCacheOptions.SectionName);

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
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JarvisCacheOptions>>().Value;
            var cacheService = new CacheService(options);
            cacheService.SetServiceProvider(sp);
            cacheService.SetMemCache(sp.GetRequiredService<IMemoryCache>());
            cacheService.SetDistributedCacheRegistry(sp.GetRequiredService<DistributedCacheRegistry>());
            return cacheService;
        });

        var snapshot = new JarvisCacheOptions();
        builder.Configuration.GetSection(JarvisCacheOptions.SectionName).Bind(snapshot);
        configure?.Invoke(snapshot);

        return new JarvisCachingBuilder(builder, snapshot);
    }
}

/// <summary>
/// Fluent follow-up after <see cref="JarvisCachingHostBuilderExtensions.AddJarvisCaching"/>.
/// </summary>
public sealed class JarvisCachingBuilder
{
    internal JarvisCachingBuilder(IHostApplicationBuilder hostBuilder, JarvisCacheOptions optionsSnapshot)
    {
        HostBuilder = hostBuilder;
        OptionsSnapshot = optionsSnapshot;
    }

    public IHostApplicationBuilder HostBuilder { get; }

    public JarvisCacheOptions OptionsSnapshot { get; }
}
