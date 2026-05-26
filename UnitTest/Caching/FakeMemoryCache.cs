using Jarvis.Caching;

namespace UnitTest.Caching;

internal sealed class FakeMemoryCache : IMemoryCache
{
    private readonly Dictionary<string, object?> _store = new();

    public int SetCount { get; private set; }

    public Task<CacheValue<T>> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(key, out var value) && value is T typed)
            return Task.FromResult(CacheValue<T>.Hit(typed));

        return Task.FromResult(CacheValue<T>.Miss());
    }

    public Task SetAsync<T>(string key, T data, TimeSpan? expires = null, CancellationToken cancellationToken = default)
    {
        SetCount++;
        _store[key] = data;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _store.Remove(key);
        return Task.CompletedTask;
    }
}
