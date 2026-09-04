using Jarvis.DDD.Domain.Services;

namespace Jarvis.Multitenancy;

/// <summary>
/// Singleton <see cref="ICurrentTenantAccessor"/> dùng <see cref="AsyncLocal{T}"/> —
/// tenant chảy qua child scope của interceptor; các HTTP request song song vẫn tách biệt.
/// </summary>
public sealed class CurrentTenantAccessor : ICurrentTenantAccessor
{
    private static readonly AsyncLocal<Guid?> Current = new();

    public Guid? TenantId => Current.Value;

    public IDisposable BeginScope(Guid tenantId)
    {
        var previous = Current.Value;
        Current.Value = tenantId;
        return new RestoreScope(previous);
    }

    private sealed class RestoreScope(Guid? previous) : IDisposable
    {
        public void Dispose() => Current.Value = previous;
    }
}
