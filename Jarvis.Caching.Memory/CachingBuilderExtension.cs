namespace Jarvis.Caching.Memory;

public static class CachingBuilderExtension
{
    public static CachingBuilder UseMemoryCache(this CachingBuilder builder)
    {
        builder.SetMemoryCache(
            new MemoryCache(
                new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions())
            )
        );

        return builder;
    }
}