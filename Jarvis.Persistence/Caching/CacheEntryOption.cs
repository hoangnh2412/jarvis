namespace Jarvis.Persistence.Caching;

public class CacheEntryOption
{
    //
    // Summary:
    //     Gets or sets an absolute expiration date for the cache entry.
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    //
    // Summary:
    //     Gets or sets an absolute expiration time, relative to now.
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    //
    // Summary:
    //     Gets or sets how long a cache entry can be inactive (e.g. not accessed) before
    //     it will be removed. This will not extend the entry lifetime beyond the absolute
    //     expiration (if set).
    public TimeSpan? SlidingExpiration { get; set; }
}