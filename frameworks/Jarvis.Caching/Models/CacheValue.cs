namespace Jarvis.Caching;

/// <summary>
/// Distinguishes cache miss from a stored value (including null references and default value types).
/// </summary>
public readonly struct CacheValue<T>
{
    public bool HasValue { get; init; }

    public T? Value { get; init; }

    public static CacheValue<T> Miss() => default;

    public static CacheValue<T> Hit(T? value) => new() { HasValue = true, Value = value };
}
