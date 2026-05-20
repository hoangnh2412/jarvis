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
}
