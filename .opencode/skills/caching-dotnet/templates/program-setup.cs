// InfrastructureLayerExtension — Jarvis Caching (memory; Redis xem workflows/add.md)

using Jarvis.Caching.Extensions;

public static IHostApplicationBuilder AddInfrastructureCaching(this IHostApplicationBuilder builder)
{
    builder.AddJarvisCaching();
  // .UseRedisDistributedCache()
  // .UseRedisMemoryCacheInvalidation();
    return builder;
}
