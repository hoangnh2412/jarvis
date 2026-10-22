using Platform.DDD.Domain.Services;

namespace Platform.MultiOrg;

/// <summary>Singleton <see cref="ICurrentOrgAccessor"/> — <see cref="AsyncLocal{T}"/>.</summary>
public sealed class CurrentOrgAccessor : ICurrentOrgAccessor
{
    private static readonly AsyncLocal<Guid?> Current = new();

    public Guid? OrgId => Current.Value;

    public IDisposable BeginScope(Guid orgId)
    {
        var previous = Current.Value;
        Current.Value = orgId;
        return new RestoreScope(previous);
    }

    private sealed class RestoreScope(Guid? previous) : IDisposable
    {
        public void Dispose() => Current.Value = previous;
    }
}
