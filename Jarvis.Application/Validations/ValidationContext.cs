using System.Collections.Concurrent;

namespace Jarvis.Application.Validations;

public class ValidationContext : IValidationContext
{
    private readonly ConcurrentDictionary<string, object> _items = new ConcurrentDictionary<string, object>();

    public ValidationContext()
    {
    }

    public void AddItem<T>(string key, T value)
    {
        _items.TryAdd(key, value);
    }

    public T GetItem<T>(string key)
    {
        if (_items.TryGetValue(key, out object value))
            return (T)value;

        return (T)value;
    }

    public T GetOrAddItem<T>(string key, Func<T> builder)
    {
        if (_items.TryGetValue(key, out object value))
            return (T)value;

        var data = builder();
        AddItem(key, data);
        return (T)data;
    }

    public async Task<T> GetOrAddItemAsync<T>(string key, Func<Task<T>> builder)
    {
        if (_items.TryGetValue(key, out object value))
            return (T)value;

        var data = await builder();
        AddItem(key, data);
        return (T)data;
    }
}