namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Singleton <see cref="ICurrentTenantAccessor"/> backed by <see cref="AsyncLocal{T}"/> so tenant flows across
/// interceptor child scopes and parallel HTTP requests stay isolated.
/// </summary>
public sealed class CurrentTenantAccessor : ICurrentTenantAccessor
{
    private static readonly AsyncLocal<Guid?> _current = new();

    public Guid? TenantId => _current.Value;

    public IDisposable BeginScope(Guid tenantId)
    {
        var previous = _current.Value;
        _current.Value = tenantId;
        return new RestoreScope(previous);
    }

    private sealed class RestoreScope(Guid? previous) : IDisposable
    {
        public void Dispose() => _current.Value = previous;
    }
}
