using Microsoft.Extensions.Configuration;
using OpenTelemetry.Trace;

namespace Jarvis.Caching.Redis.Extensions;

/// <summary>
/// Optional OpenTelemetry helpers for Jarvis caching Redis connections (requires
/// <c>OpenTelemetry.Instrumentation.StackExchangeRedis</c>).
/// </summary>
public static class RedisOpenTelemetryTraceExtensions
{
    /// <summary>
    /// Instruments the dedicated memory invalidation multiplexer registered as
    /// <see cref="MemoryCacheInvalidationDefaults.ConnectionServiceKey"/>.
    /// Does not affect other keyed or non-keyed Redis connections.
    /// </summary>
    public static TracerProviderBuilder AddJarvisCachingMemoryInvalidationRedisInstrumentation(
        this TracerProviderBuilder builder) =>
        builder.AddRedisInstrumentation(MemoryCacheInvalidationDefaults.ConnectionServiceKey);

    /// <summary>
    /// Instruments an additional keyed <c>IConnectionMultiplexer</c> without replacing existing Redis instrumentation.
    /// </summary>
    public static TracerProviderBuilder AddJarvisCachingRedisInstrumentation(
        this TracerProviderBuilder builder,
        string keyedServiceName) =>
        builder.AddRedisInstrumentation(keyedServiceName);

    /// <summary>
    /// Instruments every Redis connection under <c>Cache:DistributedGroups:Redis</c>
    /// (same multiplexers as <see cref="CachingBuilderExtension.UseRedisDistributedCache"/>).
    /// </summary>
    public static TracerProviderBuilder AddJarvisCachingDistributedRedisInstrumentation(
        this TracerProviderBuilder builder,
        JarvisCacheOptions cacheOptions)
    {
        ArgumentNullException.ThrowIfNull(cacheOptions);

        if (!cacheOptions.DistributedGroups.TryGetValue("Redis", out var groups))
            return builder;

        var manager = RedisConnectionManager.GetInstance();
        foreach (var group in groups)
        {
            if (!group.Value.TryGetValue("Configuration", out var configuration)
                || string.IsNullOrWhiteSpace(configuration))
                continue;

            var multiplexer = manager.Create(RedisConnectionPurpose.DistributedCache, configuration);
            builder.AddRedisInstrumentation(multiplexer);
        }

        return builder;
    }

    /// <summary>
    /// Binds <see cref="JarvisCacheOptions"/> from <paramref name="configuration"/> and instruments distributed Redis clusters.
    /// </summary>
    public static TracerProviderBuilder AddJarvisCachingDistributedRedisInstrumentation(
        this TracerProviderBuilder builder,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new JarvisCacheOptions();
        configuration.GetSection(JarvisCacheOptions.SectionName).Bind(options);
        return builder.AddJarvisCachingDistributedRedisInstrumentation(options);
    }
}
