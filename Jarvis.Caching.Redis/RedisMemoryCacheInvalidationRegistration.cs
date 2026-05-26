using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Jarvis.Caching.Redis;

internal static class RedisMemoryCacheInvalidationRegistration
{
    public static string ResolveConfiguration(
        IConfiguration configuration,
        JarvisCacheOptions? optionsSnapshot,
        string? explicitConfiguration)
    {
        if (!string.IsNullOrWhiteSpace(explicitConfiguration))
            return explicitConfiguration;

        var fromSnapshot = optionsSnapshot?.MemoryInvalidation.Redis.Configuration;
        if (!string.IsNullOrWhiteSpace(fromSnapshot))
            return fromSnapshot;

        var fromConfig = configuration[
            $"{JarvisCacheOptions.SectionName}:MemoryInvalidation:Redis:Configuration"];
        if (!string.IsNullOrWhiteSpace(fromConfig))
            return fromConfig;

        throw new InvalidOperationException(
            "Memory cache invalidation requires a dedicated Redis connection. " +
            $"Set {JarvisCacheOptions.SectionName}:MemoryInvalidation:Redis:Configuration " +
            "or pass the configuration string to UseRedisMemoryCacheInvalidation(configuration).");
    }

    public static void Register(
        IServiceCollection services,
        string configuration)
    {
        services.TryAddKeyedSingleton<IConnectionMultiplexer>(
            MemoryCacheInvalidationDefaults.ConnectionServiceKey,
            (_, _) => RedisConnectionManager.GetInstance().Create(
                RedisConnectionPurpose.MemoryInvalidation,
                configuration));

        services.TryAddSingleton<IMemoryCacheInvalidationPublisher>(sp =>
            new RedisMemoryCacheInvalidationPublisher(
                sp.GetRequiredKeyedService<IConnectionMultiplexer>(
                    MemoryCacheInvalidationDefaults.ConnectionServiceKey)));

        services.AddHostedService(sp =>
            new RedisMemoryCacheInvalidationSubscriber(
                sp.GetRequiredKeyedService<IConnectionMultiplexer>(
                    MemoryCacheInvalidationDefaults.ConnectionServiceKey),
                sp.GetRequiredService<IMemoryCache>(),
                sp.GetRequiredService<ILogger<RedisMemoryCacheInvalidationSubscriber>>()));
    }
}
