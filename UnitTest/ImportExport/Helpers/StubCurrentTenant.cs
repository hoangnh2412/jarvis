using Platform.DDD.Domain.Services;

namespace UnitTest.ImportExport.Helpers;

public sealed class TestTenantIdentity : ICurrentTenantIdentity
{
    public Guid? TenantId { get; init; }

    public string Name { get; init; } = string.Empty;
}

/// <summary>Test double for <see cref="ICurrentTenant{TTenant}"/> — same contract as production Setting/ImportExport.</summary>
public sealed class StubCurrentTenant : ICurrentTenant<TestTenantIdentity>
{
    private Guid? _tenantId;

    public Task<Guid?> GetIdAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_tenantId);
    }

    public Task<TestTenantIdentity?> GetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_tenantId is not Guid tenantId || tenantId == Guid.Empty)
            return Task.FromResult<TestTenantIdentity?>(null);

        return Task.FromResult<TestTenantIdentity?>(new TestTenantIdentity
        {
            TenantId = tenantId,
            Name = tenantId.ToString("D")
        });
    }

    public IDisposable Change(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        var previous = _tenantId;
        _tenantId = tenantId;
        return new TenantScope(this, previous);
    }

    public void SetTenantId(Guid? tenantId) => _tenantId = tenantId;

    private sealed class TenantScope(StubCurrentTenant owner, Guid? previous) : IDisposable
    {
        public void Dispose() => owner._tenantId = previous;
    }
}
