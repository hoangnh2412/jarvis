using Jarvis.DDD.Domain.Services;

namespace UnitTest.SignalR;

internal sealed class FakeCurrentTenant(Guid? tenantId) : ICurrentTenant<TestTenantInfo>
{
    public Task<Guid?> GetIdAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(tenantId);

    public Task<TestTenantInfo?> GetAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<TestTenantInfo?>(null);

    public IDisposable Change(Guid tenantId) => NullScope.Instance;
}
